using System.Security.Cryptography;

namespace PCRemote.Core.Security;

public class CryptoService
{
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private readonly byte[] _key;

    public CryptoService(byte[] key)
    {
        if (key.Length != 32)
            throw new ArgumentException("Key must be 32 bytes");

        _key = key;
    }

    public byte[] Encrypt(byte[] plain)
    {
        byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
        byte[] cipher = new byte[plain.Length];
        byte[] tag = new byte[TagSize];

        using var aes = new AesGcm(_key, 16);
        aes.Encrypt(nonce, plain, cipher, tag);

        return Combine(nonce, cipher, tag);
    }

    public byte[] Decrypt(byte[] data)
    {
        if (data == null || data.Length < NonceSize + TagSize)
            throw new CryptographicException("Invalid encrypted payload size");
        var nonce = data[..NonceSize];
        var tag = data[(data.Length - TagSize)..];
        var cipher = data[NonceSize..(data.Length - TagSize)];

        byte[] plain = new byte[cipher.Length];

        using var aes = new AesGcm(_key, 16);
        aes.Decrypt(nonce, cipher, tag, plain);

        return plain;
    }

    private static byte[] Combine(params byte[][] arrays)
    {
        var length = arrays.Sum(a => a.Length);
        var result = new byte[length];
        int offset = 0;

        foreach (var arr in arrays)
        {
            Buffer.BlockCopy(arr, 0, result, offset, arr.Length);
            offset += arr.Length;
        }

        return result;
    }
}