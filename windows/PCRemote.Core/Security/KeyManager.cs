using System.Security.Cryptography;

namespace PCRemote.Core.Security;

public static class KeyManager
{
    private static readonly string KeyPath =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "PCRemote",
            "key.bin");

    public static byte[] GetOrCreateKey()
    {
        if (File.Exists(KeyPath))
            return File.ReadAllBytes(KeyPath);

        Directory.CreateDirectory(Path.GetDirectoryName(KeyPath)!);

        byte[] key = RandomNumberGenerator.GetBytes(32);

        File.WriteAllBytes(KeyPath, key);

        return key;
    }
    public static string GetKeyBase64()
    {
        return Convert.ToBase64String(GetOrCreateKey());
    }
}