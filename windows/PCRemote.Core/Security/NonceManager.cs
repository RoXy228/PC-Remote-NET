using System.Collections.Concurrent;

namespace PCRemote.Core.Security;

public class NonceManager
{
    private const int MaxNonces = 5000;
    private const int TtlSeconds = 60;

    private readonly ConcurrentDictionary<string, long> _usedNonces = new();

    public bool Validate(string nonce, long timestamp)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        Cleanup(now);

        if (_usedNonces.Count >= MaxNonces)
            return false;

        return _usedNonces.TryAdd(nonce, now);
    }

    private void Cleanup(long now)
    {
        foreach (var item in _usedNonces)
        {
            if (now - item.Value > TtlSeconds)
                _usedNonces.TryRemove(item.Key, out _);
        }
    }
}