using System.Diagnostics;

namespace PCRemote.Service.SystemControl;

public class ShutdownTimer
{
    private CancellationTokenSource? _cts;

    public bool IsActive => _cts != null;

    public void StartShutdown(int minutes)
    {
        Cancel();

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(minutes), token);

                Process.Start(new ProcessStartInfo
                {
                    FileName = "shutdown",
                    Arguments = "/s /t 0",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
            }
            catch (TaskCanceledException)
            {
            }
        }, token);
    }

    public void StartReboot(int minutes)
    {
        Cancel();

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(minutes), token);

                Process.Start(new ProcessStartInfo
                {
                    FileName = "shutdown",
                    Arguments = "/r /t 0",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
            }
            catch (TaskCanceledException)
            {
            }
        }, token);
    }

    public void Cancel()
    {
        _cts?.Cancel();
        _cts = null;
    }
}