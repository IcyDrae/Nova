using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;

namespace Nova
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon? _notifyIcon;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += (_, _) =>
            {
                SearchBox.Focus();
            };

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
    }
}