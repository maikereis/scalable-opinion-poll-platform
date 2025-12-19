using System.Security.Cryptography;
using System.Text;
using Parrhesia.Domain.Common;

namespace Parrhesia.Domain.Voting.ValueObjects;

public class DeviceInfo : ValueObject
{
    public string DeviceId { get; }
    public string UserAgent { get; }
    public string IpHash { get; }

#pragma warning disable CS8618
    private DeviceInfo() { }
#pragma warning restore CS8618

    private DeviceInfo(string deviceId, string userAgent, string ipHash)
    {
        DeviceId = deviceId;
        UserAgent = userAgent;
        IpHash = ipHash;
    }

    public static DeviceInfo Create(string deviceId, string userAgent, string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("DeviceId is required", nameof(deviceId));

        var ipHash = HashIpAddress(ipAddress);

        return new DeviceInfo(deviceId, userAgent ?? "Unknown", ipHash);
    }

    private static string HashIpAddress(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return "Unknown";

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(ipAddress));
        return Convert.ToHexString(hash);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return DeviceId;
        yield return UserAgent;
        yield return IpHash;
    }
}