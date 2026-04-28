using System.Windows;

namespace PCRemote.WPF
{
    public static class QrWindowManager
    {
        private static QrWindow? _window;

        public static void Show(Window owner, string ip, int port)
        {
            if (_window != null)
                return;

            _window = new QrWindow(ip, port);
            _window.Owner = owner;
            _window.Closed += (s, e) => _window = null;
            _window.Show();
        }

        public static void CloseIfOpen()
        {
            _window?.Close();
        }
    }
}