using System;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Interop;
using System.Diagnostics;

namespace PCRemote.WPF
{
    public partial class TrayMenuWindow : Window
    {
        private readonly MainWindow _main;

        public TrayMenuWindow(MainWindow main)
        {
            InitializeComponent();
            _main = main;

            UpdateStatus();

            _main.ServiceStatusChanged += UpdateStatus;
        }

        public void UpdateStatus()
        {
            Dispatcher.Invoke(() =>
            {
                switch (_main.CurrentServiceState)
                {
                    case MainWindow.ServiceState.Running:
                        ServiceStatusText.Text = "Служба: Работает";
                        break;

                    case MainWindow.ServiceState.Stopped:
                        ServiceStatusText.Text = "Служба: Остановлена";
                        break;

                    case MainWindow.ServiceState.Restarting:
                        ServiceStatusText.Text = "Служба: перезапуск...";
                        break;

                    default:
                        ServiceStatusText.Text = "Служба: неизвестно";
                        break;
                }
            });
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            _main.ShowFromTray();
            Close();
        }

        private async void Restart_Click(object sender, RoutedEventArgs e)
        {
            ServiceStatusText.Text = "Служба: перезапуск...";

            _main.RestartServiceFromTray();

            await Task.Delay(1500);

            UpdateStatus();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            _main.ExitApplicationFromTray();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            PreparePosition();

            Activate();
            Focus();

            var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(140));

            var scale = new DoubleAnimation(0.95, 1, TimeSpan.FromMilliseconds(140))
            {
                EasingFunction = new QuadraticEase()
            };

            var slide = new DoubleAnimation(20, 0, TimeSpan.FromMilliseconds(140))
            {
                EasingFunction = new QuadraticEase()
            };

            BeginAnimation(Window.OpacityProperty, fade);

            CardScale.BeginAnimation(
                System.Windows.Media.ScaleTransform.ScaleXProperty, scale);

            CardScale.BeginAnimation(
                System.Windows.Media.ScaleTransform.ScaleYProperty, scale);

            CardSlide.BeginAnimation(
                System.Windows.Media.TranslateTransform.YProperty, slide);
        }
        public void PreparePosition()
        {
            UpdateLayout();
            Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
            Arrange(new Rect(0, 0, DesiredSize.Width, DesiredSize.Height));

            System.Windows.Point trayPoint = new System.Windows.Point(0, 0);
            bool haveTray = false;

            try
            {
                var mainHandle = new System.Windows.Interop.WindowInteropHelper(_main).Handle;
                trayPoint = TrayPositionHelper.GetTrayIconPosition(mainHandle);
                if (trayPoint.X != 0 || trayPoint.Y != 0)
                    haveTray = true;
            }
            catch
            {
                haveTray = false;
            }

            if (!haveTray)
            {
                var m = System.Windows.Forms.Control.MousePosition;
                trayPoint = new System.Windows.Point(m.X, m.Y);
            }

            var source = PresentationSource.FromVisual(_main) ?? PresentationSource.FromVisual(this);
            Matrix transformFromDevice = Matrix.Identity;
            if (source?.CompositionTarget != null)
                transformFromDevice = source.CompositionTarget.TransformFromDevice;

            var dipPoint = transformFromDevice.Transform(trayPoint);

            Debug.WriteLine($"trayPoint(device)={trayPoint.X},{trayPoint.Y}  dipPoint={dipPoint.X},{dipPoint.Y}  haveTray={haveTray}");

            double width = DesiredSize.Width;
            double height = DesiredSize.Height;

            double x = dipPoint.X - width / 2.0;
            double y = dipPoint.Y - height - 8.0;

            var work = SystemParameters.WorkArea;
            Left = Math.Max(work.Left, Math.Min(x, work.Right - width));
            Top = Math.Max(work.Top, Math.Min(y, work.Bottom - height));
        }
        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);

            if (IsLoaded && IsVisible)
                CloseWithAnimation();
        }

        protected override void OnClosed(EventArgs e)
        {
            _main.ServiceStatusChanged -= UpdateStatus;
            base.OnClosed(e);
        }

        private void CloseWithAnimation()
        {
            var fade = new DoubleAnimation(0, TimeSpan.FromMilliseconds(120));

            var slide = new DoubleAnimation(0, 20, TimeSpan.FromMilliseconds(120));

            CardSlide.BeginAnimation(
                System.Windows.Media.TranslateTransform.YProperty, slide);

            fade.Completed += (s, e) => Close();

            BeginAnimation(Window.OpacityProperty, fade);
        }
    }
}