using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using PCRemote.Core.Models;
using PCRemote.Core.Security;

var key = KeyManager.GetOrCreateKey();

var request = new RemoteRequest
{
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    Nonce = Guid.NewGuid().ToString(),
    Command = "LOCK"
};

var json = JsonSerializer.Serialize(request);
var crypto = new CryptoService(key);
var encrypted = crypto.Encrypt(Encoding.UTF8.GetBytes(json));

using var client = new TcpClient();
await client.ConnectAsync("127.0.0.1", 5055);

using var stream = client.GetStream();

var length = BitConverter.GetBytes(encrypted.Length);
await stream.WriteAsync(length);
await stream.WriteAsync(encrypted);

Console.WriteLine("Команда отправлена");
// читаем ответ
byte[] responseLengthBuffer = new byte[4];
await stream.ReadExactlyAsync(responseLengthBuffer);

int responseLength = BitConverter.ToInt32(responseLengthBuffer, 0);

byte[] responseBuffer = new byte[responseLength];
await stream.ReadExactlyAsync(responseBuffer);

var decryptedResponse = crypto.Decrypt(responseBuffer);
var responseJson = Encoding.UTF8.GetString(decryptedResponse);

Console.WriteLine($"Ответ сервера: {responseJson}");