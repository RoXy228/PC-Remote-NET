using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PCRemote.Core.Commands;
using PCRemote.Core.Logging;
using PCRemote.Core.Models;
using PCRemote.Core.Security;
using PCRemote.Service.SystemControl;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using PCRemote.Service.IPC;

namespace PCRemote.Service;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    private const int Port = 5055;

    private const int MaxPacketSize = 64 * 1024;

    private const int MaxConcurrentClients = 50;

    private readonly SemaphoreSlim _clientLimiter = new(MaxConcurrentClients);

    private readonly byte[] _key = KeyManager.GetOrCreateKey();

    private readonly FileLogger _fileLogger = new();

    private readonly NonceManager _nonceManager = new();

    private readonly RateLimiter _rateLimiter = new();

    private readonly ShutdownTimer _shutdownTimer = new();

    private bool _hasClientConnected = false;


    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Служба PCRemote запущена.");
        _fileLogger.Log("Служба PCRemote запущена.");

        FirewallManager.EnsureRule(Port);

        var listener = new TcpListener(IPAddress.Any, Port);
        listener.Start();

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync(stoppingToken);

                await _clientLimiter.WaitAsync(stoppingToken);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await HandleClient(client, stoppingToken);
                    }
                    finally
                    {
                        _clientLimiter.Release();
                    }
                }, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            listener.Stop();
        }
    }

    private async Task HandleClient(TcpClient client, CancellationToken token)
    {
        var ip = ((IPEndPoint)client.Client.RemoteEndPoint!).Address.ToString();

        _fileLogger.Log($"Получен запрос от {ip}");

        if (_rateLimiter.IsBlocked(ip))
        {
            using var stream = client.GetStream();
            var crypto = new CryptoService(_key);

            await SendResponse(stream, crypto,
                new RemoteResponse
                {
                    Ok = false,
                    ErrorCode = ErrorCodes.RateLimited,
                    Message = "Too many failed attempts"
                },
                token);

            client.Close();
            return;
        }

        try
        {
            using var stream = client.GetStream();

            byte[] lengthBuffer = new byte[4];
            await stream.ReadExactlyAsync(lengthBuffer, token);

            int length = BitConverter.ToInt32(lengthBuffer, 0);

            if (length <= 0 || length > MaxPacketSize)
            {
                _fileLogger.Log($"Отклонён пакет недопустимого размера: {length}");
                client.Close();
                return;
            }

            byte[] buffer = new byte[length];
            await stream.ReadExactlyAsync(buffer, token);

            var crypto = new CryptoService(_key);
            var decrypted = crypto.Decrypt(buffer);

            var json = Encoding.UTF8.GetString(decrypted);
            _fileLogger.Log($"Получен JSON: {json}");

            var request = JsonSerializer.Deserialize<RemoteRequest>(json);

            if (request == null)
                throw new Exception("Некорректный формат запроса");

            if (string.IsNullOrWhiteSpace(request.Command) || request.Command.Length > 50)
            {
                await SendResponse(stream, crypto,
                    new RemoteResponse
                    {
                        Ok = false,
                        ErrorCode = ErrorCodes.InvalidPacket,
                        Message = "Invalid command"
                    },
                    token);

                return;
            }

            if (request.Version != 1)
            {
                _fileLogger.Log($"Отклонён запрос с неподдерживаемой версией: {request.Version}");
                await SendResponse(stream, crypto,
                new RemoteResponse
                {
                    Ok = false,
                    ErrorCode = ErrorCodes.InvalidVersion,
                    Message = "Unsupported protocol version"
                },
                token);

                return;
            }

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var diff = Math.Abs(now - request.Timestamp);

            if (diff > 30)
            {
                _fileLogger.Log("Запрос отклонён: превышено допустимое окно времени.");

                await SendResponse(stream, crypto,
                    new RemoteResponse
                    {
                        Ok = false,
                        ErrorCode = ErrorCodes.ExpiredRequest,
                        Message = "Request expired"
                    },
                    token);

                return;
            }


            if (!_nonceManager.Validate(request.Nonce, request.Timestamp))
            {
                _fileLogger.Log("Запрос отклонён: повторный nonce.");

                await SendResponse(stream, crypto,
                    new RemoteResponse
                    {
                        Ok = false,
                        ErrorCode = ErrorCodes.ReplayDetected,
                        Message = "Replay attack detected"
                    },
                    token);

                return;
            }

            _rateLimiter.RegisterSuccess(ip);

            if (!_hasClientConnected)
            {
                _hasClientConnected = true;
                PipeClient.Send("CONNECTED");
            }

            string result;

            var cmd = request.Command.ToUpper();

            if (cmd.StartsWith("SHUTDOWN:"))
            {
                var minutes = int.Parse(cmd.Split(':')[1]);
                _shutdownTimer.StartShutdown(minutes);
                result = $"Выключение через {minutes} мин.";
            }
            else if (cmd.StartsWith("REBOOT:"))
            {
                var minutes = int.Parse(cmd.Split(':')[1]);
                _shutdownTimer.StartReboot(minutes);
                result = $"Перезагрузка через {minutes} мин.";
            }
            else if (cmd == "CANCEL")
            {
                _shutdownTimer.Cancel();
                result = "Таймер отменён";
            }
            else if (cmd == "SHUTDOWN")
            {
                _shutdownTimer.Cancel();
                CommandExecutor.ShutdownNow();
                result = "Выключение";
            }
            else if (cmd == "REBOOT")
            {
                _shutdownTimer.Cancel();
                CommandExecutor.RebootNow();
                result = "Перезагрузка";
            }
            else if (cmd == "STATUS")
            {
                await SendResponse(stream, crypto,
                    new RemoteResponse
                    {
                        Ok = true
                    },
                    token);

                return;
            }
            else
            {

                PipeClient.Send(cmd);
                result = "Команда передана агенту";
            }

            var response = new RemoteResponse
            {
                Ok = true,
                Message = result
            };

            var responseJson = JsonSerializer.Serialize(response);

            var encryptedResponse = crypto.Encrypt(Encoding.UTF8.GetBytes(responseJson));

            byte[] responseLength = BitConverter.GetBytes(encryptedResponse.Length);

            await stream.WriteAsync(responseLength, token);
            await stream.WriteAsync(encryptedResponse, token);
        }
        catch (Exception ex)
        {
            _rateLimiter.RegisterFailure(ip);

            _logger.LogError(ex, "Ошибка при обработке клиента");
            _fileLogger.LogError(ex.ToString());
        }
        finally
        {
            client.Close();
        }
    }
    private async Task SendResponse(NetworkStream stream, CryptoService crypto, RemoteResponse response, CancellationToken token)
    {
        var json = JsonSerializer.Serialize(response);
        var encrypted = crypto.Encrypt(Encoding.UTF8.GetBytes(json));

        byte[] length = BitConverter.GetBytes(encrypted.Length);

        await stream.WriteAsync(length, token);
        await stream.WriteAsync(encrypted, token);
    }
}