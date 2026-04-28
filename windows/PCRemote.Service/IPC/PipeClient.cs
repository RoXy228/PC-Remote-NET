using System.IO.Pipes;
using System.Text;

namespace PCRemote.Service.IPC;

public static class PipeClient
{
    private const string PipeName = "PCRemotePipe";

    public static void Send(string command)
    {
        try
        {
            using var client = new NamedPipeClientStream(
                ".",
                PipeName,
                PipeDirection.Out);

            client.Connect(1000);

            var bytes = Encoding.UTF8.GetBytes(command);
            client.Write(bytes, 0, bytes.Length);
        }
        catch
        {

        }
    }
}