namespace PCRemote.Core.Models;

public static class ErrorCodes
{
    public const string InvalidPacket = "INVALID_PACKET";
    public const string InvalidCrypto = "INVALID_CRYPTO";
    public const string InvalidVersion = "INVALID_VERSION";
    public const string ExpiredRequest = "EXPIRED_REQUEST";
    public const string ReplayDetected = "REPLAY_DETECTED";
    public const string RateLimited = "RATE_LIMITED";
    public const string InternalError = "INTERNAL_ERROR";
}