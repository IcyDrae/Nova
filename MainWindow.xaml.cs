using Nova.Models;
using Nova.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
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

        private List<ApplicationEntry> Apps = new();
        private readonly StartMenuScannerService StartMenuScannerService = new();
        private ObservableCollection<ApplicationEntry> FilteredApps = new();

        private const double BaseHeight = 60;   // search box area
        private const double ItemHeight = 42;   // each result row
        private const double MaxVisibleItems = 8;

        public MainWindow()
        {
            InitializeComponent();
            InitializeSearchBox();
            InitializeNotifyIcon();

            Apps = new StartMenuScannerService()
                .Scan()
                .GroupBy(a => a.Name)
                .Select(g => g.First())
                .OrderBy(App => App.Name)
                .ToList();
            FilteredApps = new ObservableCollection<ApplicationEntry>();
            ResultsList.ItemsSource = FilteredApps;

            FilteredApps.Clear();
            ResultsList.SelectedIndex = 0;
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

            SearchBox.Text = "";

            SearchBox.Dispatcher.BeginInvoke(new Action(() =>
            {
                SearchBox.Focus();
                Keyboard.Focus(SearchBox);
            }));
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

            if (FilteredApps.Count == 0)
                return;

            if (e.Key == Key.Down)
            {
                if (ResultsList.SelectedIndex < FilteredApps.Count - 1)
                    ResultsList.SelectedIndex++;
                else
                    ResultsList.SelectedIndex = 0;

                ResultsList.ScrollIntoView(ResultsList.SelectedItem);
                e.Handled = true;
            }

            if (e.Key == Key.Up)
            {
                if (ResultsList.SelectedIndex > 0)
                    ResultsList.SelectedIndex--;
                else
                    ResultsList.SelectedIndex = FilteredApps.Count - 1;

                ResultsList.ScrollIntoView(ResultsList.SelectedItem);
                e.Handled = true;
            }

            if (e.Key == Key.Enter)
            {
                LaunchSelectedApp();
                e.Handled = true;
                return;
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

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

        private void FilterApps(string query)
        {
            FilteredApps.Clear();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var results = Apps.Where(a =>
                    a.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(App => App.Name);

                foreach (var app in results)
                    FilteredApps.Add(app);
            }

            ResultsList.SelectedIndex = 0;

            ResultsList.Visibility = FilteredApps.Count == 0
                ? Visibility.Collapsed
                : Visibility.Visible;

            UpdateWindowHeight();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholder();
            FilterApps(SearchBox.Text);
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            Hide();
        }

        private void UpdateWindowHeight()
        {
            int count = FilteredApps.Count;

            if (count == 0)
            {
                Height = BaseHeight;
                return;
            }

            int visibleItems = Math.Min(count, (int)MaxVisibleItems);

            double listHeight = visibleItems * ItemHeight;

            // 👇 prevents “cramped single item” look
            if (count == 1)
                listHeight = ItemHeight + 10;

            Height = BaseHeight + listHeight;
        }

        private void LaunchSelectedApp()
        {
            if (ResultsList.SelectedItem is not ApplicationEntry app)
                return;

            try
            {
                if (app.Path.StartsWith("shell:AppsFolder"))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = app.Path,
                        UseShellExecute = true,
                        WorkingDirectory = Environment.GetFolderPath(
                            Environment.SpecialFolder.UserProfile)
                    });
                }
                else
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = app.Path,
                        UseShellExecute = true,
                        WorkingDirectory =
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.UserProfile)
                    });
                }

                HideNova();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to launch {app.Name}\n\n{ex.Message}");
            }
        }

        private void UpdatePlaceholder()
        {
            PlaceholderText.Visibility =
                string.IsNullOrWhiteSpace(SearchBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void ResultsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            LaunchSelectedApp();
        }
    }
}
