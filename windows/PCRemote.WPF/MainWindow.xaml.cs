using Microsoft.VisualBasic.Devices;
using Microsoft.Win32;
using PCRemote.Core.Networking;
using System.Diagnostics;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Brushes = System.Windows.Media.Brushes;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;


namespace PCRemote.WPF
{
    public partial class MainWindow : Window
    {
        public event Action? ServiceStatusChanged;
        private DispatcherTimer _statusTimer = new();
        private Forms.NotifyIcon? _trayIcon;
        private bool _isExitRequested = false;
        private TrayMenuWindow? _trayMenu;
        private bool _networkReady = false;
        private bool _isRefreshingNetwork;
        private static readonly HttpClient _http = new()
        {
            Timeout = TimeSpan.FromSeconds(3)
        };

        public MainWindow()
        {
            InitializeComponent();
            InitializeTray();
        }
        private void InitializeTray()
        {
            _trayIcon = new Forms.NotifyIcon();
            _trayIcon.Text = "PC Remote";

            var stream = System.Windows.Application.GetResourceStream(
                new Uri("pack://application:,,,/icon.ico"));

            _trayIcon.Icon = new System.Drawing.Icon(stream.Stream);

            _trayIcon.Visible = true;
            _trayIcon.MouseUp += TrayIcon_MouseUp;
        }
        private void TrayIcon_MouseUp(object? sender, Forms.MouseEventArgs e)
        {
            if (e.Button == Forms.MouseButtons.Right)
            {
                ShowTrayMenu();
            }
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CheckServiceStatus();
            _ = RefreshAllNetworkWithRetryAsync();

            _statusTimer.Interval = TimeSpan.FromSeconds(3);
            _statusTimer.Tick += async (s, e) =>
            {
                CheckServiceStatus();

                var net = NetworkUtils.GetNetworkInfo();

                bool hasLocal = !string.IsNullOrWhiteSpace(net.IP) && net.IP != "0.0.0.0";

                if (!hasLocal || !_networkReady)
                    await RefreshAllNetworkWithRetryAsync();
            };

            _statusTimer.Start();

            if (App.IsAutoStart)
            {
                Hide();
            }
        }

        private void CheckServiceStatus()
        {
            try
            {
                using var sc = new ServiceController("PCRemoteService");
                sc.Refresh();

                if (sc.Status == ServiceControllerStatus.Running)
                    CurrentServiceState = ServiceState.Running;
                else
                    CurrentServiceState = ServiceState.Stopped;
            }
            catch
            {
                CurrentServiceState = ServiceState.Unknown;
            }

            UpdateLocalStatusUI();
            ServiceStatusChanged?.Invoke();

            if (CurrentServiceState == ServiceState.Restarting)
                _statusTimer.Interval = TimeSpan.FromMilliseconds(500);
            else
                _statusTimer.Interval = TimeSpan.FromSeconds(2);
        }
        private async Task<IpFullInfo?> GetExternalInfoAsync()
        {
            var urls = new[]
            {
        "http://ip-api.com/json/?fields=query,countryCode",
        "https://ipinfo.io/json",
        "https://api.ipify.org?format=json"
    };

            foreach (var url in urls)
            {
                try
                {
                    var json = await _http.GetStringAsync(url);

                    // ip-api
                    if (url.Contains("ip-api"))
                    {
                        var data = JsonSerializer.Deserialize<IpFullInfo>(json);
                        if (data != null && !string.IsNullOrWhiteSpace(data.query))
                            return data;
                    }

                    // ipinfo
                    if (url.Contains("ipinfo"))
                    {
                        var doc = JsonDocument.Parse(json);
                        return new IpFullInfo
                        {
                            query = doc.RootElement.GetProperty("ip").GetString(),
                            countryCode = doc.RootElement.GetProperty("country").GetString()
                        };
                    }

                    // ipify
                    if (url.Contains("ipify"))
                    {
                        var doc = JsonDocument.Parse(json);
                        return new IpFullInfo
                        {
                            query = doc.RootElement.GetProperty("ip").GetString(),
                            countryCode = "--"
                        };
                    }
                }
                catch
                {
                    // пробуем следующий
                }
            }

            return null;
        }

        private async Task RefreshAllNetworkWithRetryAsync()
        {
            if (_isRefreshingNetwork)
                return;

            _isRefreshingNetwork = true;
            _networkReady = false;

            try
            {
                IpValue.Text = "ожидание сети...";
                CountryCode.Text = "";

                for (int i = 0; i < 30; i++)
                {
                    var data = await GetExternalInfoAsync();
                    var net = NetworkUtils.GetNetworkInfo();

                    bool hasLocal = !string.IsNullOrWhiteSpace(net.IP) && net.IP != "0.0.0.0";
                    var externalIp = data?.query;
                    bool hasExternal = !string.IsNullOrWhiteSpace(externalIp);

                    if (hasLocal && hasExternal)
                    {
                        _networkReady = true;

                        var ip = externalIp!;
                        var country = data?.countryCode ?? "--";

                        IpValue.Text = ip;
                        CountryCode.Text = country;
                        IpValue.Foreground = GetIpColor(ip, net.IP ?? "");

                        IpTextlocal.Text = $"Локальный IP: {net.IP}";
                        MAC.Text = $"MAC адрес: {net.MAC}";
                        InterfaceTypeText.Text = $"Интерфейс: {net.Type}";
                        return;
                    }

                    await Task.Delay(1000);
                }

                IpValue.Text = "нет сети";
                IpValue.Foreground = Brushes.Red;
            }
            finally
            {
                _isRefreshingNetwork = false;
            }
        }

        private bool IsBadIp(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return true;

            var parts = ip.Split('.');
            if (parts.Length != 4) return true;

            if (!int.TryParse(parts[0], out int a) ||
                !int.TryParse(parts[1], out int b))
                return true;

            return
                // Private
                a == 10 ||
                (a == 172 && b >= 16 && b <= 31) ||

                // CGNAT
                (a == 100 && b >= 64 && b <= 127) ||

                // Link-local
                (a == 169 && b == 254) ||

                // Loopback
                a == 127 ||

                // Reserved / мусор
                a == 0 ||
                a >= 224 ||

                // тестовые диапазоны
                (a == 198 && (b == 18 || b == 19)) ||
                (a == 192 && b == 0);
        }

        private System.Windows.Media.Brush GetIpColor(string externalIp, string localIp)
        {
            if (IsBadIp(externalIp))
                return Brushes.Red;

            if (localIp.StartsWith("10."))
                return Brushes.Orange; 

            return Brushes.LimeGreen;
        }

        private async void RefreshNetwork_Click(object sender, RoutedEventArgs e)
        {
            await RefreshAllNetworkWithRetryAsync();
        }

        private async void RestartService_Click(object sender, RoutedEventArgs e)
        {
            CurrentServiceState = ServiceState.Restarting;
            UpdateLocalStatusUI();
            ServiceStatusChanged?.Invoke();

            await Task.Run(() => RunElevated("restart-service"));

            await Task.Delay(1500);
            CheckServiceStatus();
        }

        private void StopService_Click(object sender, RoutedEventArgs e)
        {
            RunElevated("stop-service");
        }
        private void RunElevated(string argument)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = Process.GetCurrentProcess().MainModule!.FileName!,
                    Arguments = argument,
                    Verb = "runas",
                    UseShellExecute = true
                };

                Process.Start(psi);
            }
            catch
            {
                MessageBox.Show("Требуются права администратора.");
            }
        }
        private void ShowQr_Click(object sender, RoutedEventArgs e)
        {
            string ip = IpValue.Text;

            if (string.IsNullOrWhiteSpace(ip) || ip == "недоступен" || ip == "ожидание сети..." || ip == "нет сети")
            {
                MessageBox.Show("Внешний IP недоступен.");
                return;
            }

            var qr = new QrWindow(ip, 5055);
            qr.Owner = this;
            qr.ShowDialog();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _statusTimer.Stop();
        }
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            HideToTray();
        }
        private void HideToTray()
        {
            Hide();
        }

        private void OpenGuide_Click(object sender, RoutedEventArgs e)
        {
            var path = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "guide.html");

            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!_isExitRequested)
            {
                e.Cancel = true;
                HideToTray();
            }

            base.OnClosing(e);
        }
        public string GetServiceStatusText()
        {
            try
            {
                using var sc = new ServiceController("PCRemoteService");
                return sc.Status == ServiceControllerStatus.Running
                    ? "Работает"
                    : "Остановлена";
            }
            catch
            {
                return "Не установлена";
            }
        }
        private void ShowTrayMenu()
        {
            if (_trayMenu != null)
            {
                _trayMenu.Close();
                return;
            }

            _trayMenu = new TrayMenuWindow(this);

            var handle = new WindowInteropHelper(this).Handle;
            var pos = TrayPositionHelper.GetTrayIconPosition(handle);

            _trayMenu.Left = pos.X - _trayMenu.Width / 2;
            _trayMenu.Top = pos.Y - _trayMenu.Height - 8;

            _trayMenu.Show();

            _trayMenu.Closed += (s, e) => _trayMenu = null;
        }
        public void ShowFromTray()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }
        public async void RestartServiceFromTray()
        {
            CurrentServiceState = ServiceState.Restarting;

            _statusTimer.Interval = TimeSpan.FromMilliseconds(500);

            UpdateLocalStatusUI();
            ServiceStatusChanged?.Invoke();

            RunElevated("restart-service");

            await Task.Delay(2000);

            CheckServiceStatus();
        }
        public void ExitApplicationFromTray()
        {
            _isExitRequested = true;
            _trayIcon!.Visible = false;
            _trayIcon.Dispose();
            System.Windows.Application.Current.Shutdown();
        }
        public enum ServiceState
        {
            Unknown,
            Running,
            Stopped,
            Restarting
        }
        public ServiceState CurrentServiceState { get; private set; }
        private void UpdateLocalStatusUI()
        {
            Dispatcher.Invoke(() =>
            {
                switch (CurrentServiceState)
                {
                    case ServiceState.Running:
                        ServiceStatusText.Text = "Работает";
                        StatusDot.Fill = Brushes.LimeGreen;
                        break;

                    case ServiceState.Stopped:
                        ServiceStatusText.Text = "Остановлена";
                        StatusDot.Fill = Brushes.Red;
                        break;

                    case ServiceState.Restarting:
                        ServiceStatusText.Text = "Перезапуск службы...";
                        StatusDot.Fill = Brushes.Orange;
                        break;

                    default:
                        ServiceStatusText.Text = "Служба не установлена";
                        StatusDot.Fill = Brushes.Gray;
                        break;
                }
            });
        }
    }
}