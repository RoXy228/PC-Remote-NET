using System.Diagnostics;

namespace PCRemote.Service.SystemControl;

public static class FirewallManager
{
    private const string RulePrefix = "PCRemote";

    public static void EnsureRule(int port)
    {
        RemoveAllRules();
        AddRule(port);
    }

    private static void AddRule(int port)
    {
        Execute($"advfirewall firewall add rule name=\"{RulePrefix} {port}\" dir=in action=allow protocol=TCP localport={port}");
    }

    private static void RemoveAllRules()
    {
        Execute($"advfirewall firewall delete rule name=all | find \"{RulePrefix}\"");
    }

    private static void Execute(string args)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "netsh",
            Arguments = args,
            CreateNoWindow = true,
            UseShellExecute = false
        })?.WaitForExit();
    }
}