using System.Text;

namespace PCRemote.Core.Logging;

public class FileLogger
{
    private readonly string _logPath;
    private readonly object _lock = new();

    private const long MaxSizeBytes = 5 * 1024 * 1024; // 5 MB

    public FileLogger()
    {
        var folder = @"C:\ProgramData\PCRemote";

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        _logPath = Path.Combine(folder, "logs.txt");
    }

    public void Log(string message)
    {
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}";

        lock (_lock)
        {
            RotateIfNeeded();
            File.AppendAllText(_logPath, line + Environment.NewLine, Encoding.UTF8);
        }
    }

    public void LogError(string message)
    {
        Log("ОШИБКА: " + message);
    }

    private void RotateIfNeeded()
    {
        if (!File.Exists(_logPath))
            return;

        var info = new FileInfo(_logPath);

        if (info.Length < MaxSizeBytes)
            return;

        var lines = File.ReadAllLines(_logPath).ToList();

        int removeCount = lines.Count / 10;

        if (removeCount <= 0)
            removeCount = 1;

        lines.RemoveRange(0, removeCount);

        File.WriteAllLines(_logPath, lines, Encoding.UTF8);
    }
}