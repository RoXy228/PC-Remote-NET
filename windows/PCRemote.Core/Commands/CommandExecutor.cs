using System.Diagnostics;

namespace PCRemote.Core.Commands;

public static class CommandExecutor
{
    public static string ExecuteImmediate(string command)
    {
        switch (command.ToUpper())
        {
            case "STATUS":
                return "ПК доступен";

            case "SHUTDOWN":
                ShutdownNow();
                return "Выключение";

            case "REBOOT":
                RebootNow();
                return "Перезагрузка";

            default:
                return "UI_COMMAND";
        }
    }

    public static void ShutdownNow()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "shutdown",
            Arguments = "/s /t 0",
            CreateNoWindow = true,
            UseShellExecute = false
        });
    }

    public static void RebootNow()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "shutdown",
            Arguments = "/r /t 0",
            CreateNoWindow = true,
            UseShellExecute = false
        });
    }
}