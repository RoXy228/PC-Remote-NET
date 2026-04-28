using System;
using System.ServiceProcess;
using System.Threading;
using System.Windows;
using PCRemote.WPF.IPC;
using Microsoft.Win32;
using System.Diagnostics;
using Application = System.Windows.Application;

namespace PCRemote.WPF
{
    public partial class App : Application
    {
        private static Mutex? _mutex;
        private PipeServer _pipeServer = new();
        public static bool IsAutoStart { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                if (e.Args[0] == "--autostart")
                {
                    IsAutoStart = true;
                }
                else
                {
                    HandleElevatedCommand(e.Args);
                    Shutdown();
                    return;
                }
            }

            bool createdNew;
            _mutex = new Mutex(true, "PCRemote_SingleInstance", out createdNew);

            if (!createdNew)
            {
                Shutdown();
                return;
            }

            EnsureAutoStart(); 

            base.OnStartup(e);

            _pipeServer.Start();
        }
        private void EnsureAutoStart()
        {
            const string name = "PCRemote";

            using var key = Registry.CurrentUser.CreateSubKey(
                 @"Software\Microsoft\Windows\CurrentVersion\Run");

            if (key == null)
                return;

            string exePath = Process.GetCurrentProcess().MainModule!.FileName!;
            string expected = "\"" + exePath + "\" --autostart";

            var current = key.GetValue(name)?.ToString();

            if (current != expected)
            {
                key.SetValue(name, expected);
            }
        }
        private void HandleElevatedCommand(string[] args)
        {
            try
            {
                if (args[0] == "restart-service")
                {
                    using var sc = new ServiceController("PCRemoteService");

                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    }

                    sc.Start();
                }

                if (args[0] == "stop-service")
                {
                    using var sc = new ServiceController("PCRemoteService");

                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    }
                }
            }
            catch
            {

            }
        }
    }
}