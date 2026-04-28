using System.Collections.Concurrent;
using System.Threading;

namespace PCRemote.Core.Security;

public class RateLimiter
{
    private const int MaxFails = 5;
    private const int BlockMinutes = 2;
    private const int CleanupMinutes = 10;

    private class AttemptInfo
    {
        public int FailCount;
        public DateTime BlockedUntil;
        public DateTime LastAttempt;
    }

    private readonly ConcurrentDictionary<string, AttemptInfo> _attempts = new();

    public bool IsBlocked(string ip)
    {
        if (!_attempts.TryGetValue(ip, out var info))
            return false;

        if (info.BlockedUntil > DateTime.UtcNow)
            return true;

        return false;
    }

    public void RegisterFailure(string ip)
    {
        var now = DateTime.UtcNow;

        var info = _attempts.GetOrAdd(ip, _ => new AttemptInfo());

        Interlocked.Increment(ref info.FailCount);
        info.LastAttempt = now;

        if (info.FailCount >= MaxFails)
        {
            info.BlockedUntil = now.AddMinutes(BlockMinutes);
            info.FailCount = 0;
        }

        Cleanup(now);
    }

    public void RegisterSuccess(string ip)
    {
        _attempts.TryRemove(ip, out _);
    }

    private void Cleanup(DateTime now)
    {
        foreach (var item in _attempts)
        {
            if (item.Value.BlockedUntil < now &&
                (now - item.Value.LastAttempt).TotalMinutes > CleanupMinutes)
            {
                _attempts.TryRemove(item.Key, out _);
            }
        }
    }
}