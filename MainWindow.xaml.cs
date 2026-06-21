using Nova.Services;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace Nova
{
    public partial class MainWindow : System.Windows.Window
    {
        private NotifyIcon? _notifyIcon;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(
            IntPtr hWnd,
            int id,
            uint fsModifiers,
            uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(
            IntPtr hWnd,
            int id);

        private const int HOTKEY_ID = 9000;
        private const uint MOD_ALT = 0x0001;
        private const uint VK_SPACE = 0x20;

        private IntPtr WndProc(
            IntPtr hwnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam,
            ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;

            if (msg == WM_HOTKEY)
            {
                ShowNova();

                handled = true;
            }

            return IntPtr.Zero;
        }

        public MainWindow()
        {
            InitializeComponent();
            InitializeSearchBox();
            InitializeNotifyIcon();

            Loaded += MainWindow_Loaded;
        }

        private void InitializeSearchBox()
        {
            Loaded += (_, _) =>
            {
                SearchBox.Focus();
            };
        }

        private void InitializeNotifyIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon("Assets/nova.ico"),
                Visible = true,
                Text = "Nova"
            };

            _notifyIcon.ContextMenuStrip = new ContextMenuStrip();

            _notifyIcon.ContextMenuStrip.Items.Add(
                "Show Nova",
                null,
                (_, _) => ShowNova());

            _notifyIcon.DoubleClick += (_, _) => ShowNova();

            _notifyIcon.ContextMenuStrip.Items.Add(
                "Exit",
                null,
                (_, _) => System.Windows.Application.Current.Shutdown());
        }

        private void ShowNova()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void HideNova()
        {
            Hide();
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                HideNova();
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var Helper = new System.Windows.Interop.WindowInteropHelper(this);

            RegisterHotKey(
                Helper.Handle,
                HOTKEY_ID,
                MOD_ALT,
                VK_SPACE);

            var Source = System.Windows.Interop.HwndSource.FromHwnd(Helper.Handle);

            Source.AddHook(WndProc);
        }

        protected override void OnClosed(EventArgs e)
        {
            var Helper = new System.Windows.Interop.WindowInteropHelper(this);

            UnregisterHotKey(
                Helper.Handle,
                HOTKEY_ID);

            base.OnClosed(e);
        }
    }
}
