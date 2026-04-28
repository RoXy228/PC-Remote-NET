using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace PCRemote.Core.Networking;

public class NetworkInfo
{
    public string IP { get; set; } = "0.0.0.0";
    public string MAC { get; set; } = "00:00:00:00:00:00";
    public string Type { get; set; } = "Не найден";
}

public static class NetworkUtils
{
    public static NetworkInfo GetNetworkInfo()
    {
        var interfaces = NetworkInterface.GetAllNetworkInterfaces()
            .Where(n =>
                n.OperationalStatus == OperationalStatus.Up &&
                n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                n.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                !IsVirtual(n))
            .ToList();

        if (interfaces.Count == 0)
            return new NetworkInfo();

        var withGateway = interfaces
            .Where(i => i.GetIPProperties().GatewayAddresses.Any(g =>
                g.Address.AddressFamily == AddressFamily.InterNetwork))
            .ToList();

        if (withGateway.Any())
            interfaces = withGateway;

        var selected =
            interfaces.FirstOrDefault(i => i.NetworkInterfaceType == NetworkInterfaceType.Ethernet) ??
            interfaces.FirstOrDefault(i => i.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) ??
            interfaces.First();

        var ip = selected.GetIPProperties().UnicastAddresses
            .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork);

        return new NetworkInfo
        {
            IP = ip?.Address.ToString() ?? "0.0.0.0",
            MAC = FormatMac(selected.GetPhysicalAddress()),
            Type = GetTypeName(selected.NetworkInterfaceType)
        };
    }

    private static string GetTypeName(NetworkInterfaceType type)
    {
        return type switch
        {
            NetworkInterfaceType.Ethernet => "LAN",
            NetworkInterfaceType.Wireless80211 => "Wi-Fi",
            _ => type.ToString()
        };
    }

    private static bool IsVirtual(NetworkInterface ni)
    {
        var name = ni.Name.ToLower();
        var desc = ni.Description.ToLower();

        return desc.Contains("virtual") ||
               desc.Contains("vmware") ||
               desc.Contains("hyper-v") ||
               desc.Contains("loopback") ||
               desc.Contains("vpn") ||
               desc.Contains("tap") ||
               desc.Contains("tunnel") ||
               desc.Contains("bluetooth") ||
               name.Contains("virtual") ||
               name.Contains("vpn") ||
               name.Contains("meta") ||
               name.Contains("Mihomo") ||
               name.Contains("tap");
    }

    private static string FormatMac(PhysicalAddress addr)
    {
        var bytes = addr.GetAddressBytes();
        return bytes.Length == 0
            ? "00:00:00:00:00:00"
            : string.Join(":", bytes.Select(b => b.ToString("X2")));
    }
}