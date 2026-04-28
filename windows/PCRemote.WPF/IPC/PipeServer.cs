using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfApp = System.Windows.Application;

namespace PCRemote.WPF.IPC;

public class PipeServer
{
    private const string PipeName = "PCRemotePipe";

    [DllImport("user32.dll")]
    private static extern bool LockWorkStation();

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(
        IntPtr hWnd,
        int Msg,
        IntPtr wParam,
        IntPtr lParam);

    private const int HWND_BROADCAST = 0xffff;
    private const int WM_SYSCOMMAND = 0x0112;
    private const int SC_MONITORPOWER = 0xF170;

    public void Start()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                using var server = new NamedPipeServerStream(
                    PipeName,
                    PipeDirection.In);

                await server.WaitForConnectionAsync();

                using var reader = new StreamReader(server, Encoding.UTF8);
                var command = await reader.ReadToEndAsync();

                Execute(command);
            }
        });
    }

    private void Execute(string command)
    {
        switch (command.ToUpper())
        {
            case "LOCK":
                LockWorkStation();
                break;

            case "SCREEN_OFF":
                SendMessage((IntPtr)HWND_BROADCAST,
                    WM_SYSCOMMAND,
                    (IntPtr)SC_MONITORPOWER,
                    (IntPtr)2);
                break;
            case "CONNECTED":
                WpfApp.Current?.Dispatcher?.BeginInvoke(() =>
                {
                    QrWindowManager.CloseIfOpen();
                });
                break;
        }
    }
}