using PCRemote.Core.Networking;
using PCRemote.Core.Security;
using QRCoder;
using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace PCRemote.WPF
{
    public partial class QrWindow : Window
    {
        public QrWindow(string ip, int port)
        {
            InitializeComponent();
            GenerateQr(ip, port);
            Loaded += QrWindow_Loaded;
        }

        private void GenerateQr(string ip, int port)
        {
            string key = KeyManager.GetKeyBase64();

            var net = NetworkUtils.GetNetworkInfo();

            var payload = new
            {
                external_ip = ip,
                local_ip = net.IP,
                port = port,
                key = key,
                mac = net.MAC
            };

            string json = JsonSerializer.Serialize(payload);

            using var generator = new QRCodeGenerator();
            var data = generator.CreateQrCode(json, QRCodeGenerator.ECCLevel.H);
            var qr = new PngByteQRCode(data);

            // QR цвет
            byte[] darkColor = { 46, 123, 214, 255 };   // #2E7BD6

         
            byte[] lightColor = { 0, 0, 0, 0 }; // прозрачный фон

            byte[] bytes = qr.GetGraphic(
                 28,
                 darkColor,
                 lightColor,
                 true
            );
            using var ms = new MemoryStream(bytes);

            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = ms;
            image.EndInit();

            QrImage.Source = image;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWithAnimation();
        }
        private void QrWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Focus(); 

            if (Owner != null)
                Owner.Effect = new BlurEffect { Radius = 10 };

            var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180));
            BeginAnimation(Window.OpacityProperty, fade);
        }
        private void CloseWithAnimation()
        {
            var fade = new DoubleAnimation(0, TimeSpan.FromMilliseconds(150));
            fade.Completed += (s, e) =>
            {
                if (Owner != null)
                    Owner.Effect = null;

                Close();
            };

            BeginAnimation(Window.OpacityProperty, fade);
        }
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                CloseWithAnimation();
        }
    }
}