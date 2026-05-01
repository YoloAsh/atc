using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;

namespace atc
{
    public partial class TelegramSetupWindow : Window
    {
        // Frozen brushes — allocated once, no GC pressure, thread-safe
        private static readonly SolidColorBrush GreenBrush = CreateFrozen(0x6A, 0xBF, 0x69);
        private static readonly SolidColorBrush GrayBrush  = CreateFrozen(0x88, 0x88, 0x88);
        private static readonly SolidColorBrush BlueBrush  = CreateFrozen(0x5B, 0x9B, 0xD5);
        private static readonly SolidColorBrush RedBrush   = CreateFrozen(0xD5, 0x5B, 0x5B);

        private static SolidColorBrush CreateFrozen(byte r, byte g, byte b)
        {
            var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            brush.Freeze();
            return brush;
        }

        private string _currentPin = "";
        private string? _linkedChatId;

        public TelegramSetupWindow()
        {
            InitializeComponent();
            GeneratePin();
            LoadSettings();
        }

        private void GeneratePin()
        {
            _currentPin = Random.Shared.Next(1000, 10000).ToString();
            txtPin.Text = _currentPin;
        }

        private void LoadSettings()
        {
            var settings = TelegramSettings.Load();
            chkEnable.IsChecked = settings.IsEnabled;
            _linkedChatId = settings.ChatId;

            UpdateStatusDisplay();
        }

        private void UpdateStatusDisplay()
        {
            if (!string.IsNullOrWhiteSpace(_linkedChatId))
            {
                txtStatus.Text = $"✅ Linked (Chat ID: {_linkedChatId})";
                txtStatus.Foreground = GreenBrush;
            }
            else
            {
                txtStatus.Text = "⚠ Not Linked";
                txtStatus.Foreground = GrayBrush;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private async void btnLink_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "🔍 Detecting...";
            txtStatus.Foreground = BlueBrush;
            btnLink.IsEnabled = false;

            string? chatId = await TelegramService.GetChatIdFromPinAsync(_currentPin);

            if (!string.IsNullOrEmpty(chatId))
            {
                _linkedChatId = chatId;
                UpdateStatusDisplay();
                MessageBox.Show("Successfully linked your Telegram!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                _linkedChatId = null;
                UpdateStatusDisplay();
                txtStatus.Text = "❌ PIN not found";
                txtStatus.Foreground = RedBrush;
                MessageBox.Show(
                    $"Could not find PIN \"{_currentPin}\" in recent messages.\n\n" +
                    "Make sure you:\n" +
                    "1. Opened @missDiligenceBot in Telegram\n" +
                    "2. Sent the exact PIN shown above\n" +
                    "3. Clicked this button shortly after",
                    "Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            btnLink.IsEnabled = true;
        }

        private async void btnTest_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_linkedChatId))
            {
                MessageBox.Show("Please link your Telegram first.", "Not Linked",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            btnTest.IsEnabled = false;
            bool success = await TelegramService.SendTestMessageAsync(_linkedChatId);
            btnTest.IsEnabled = true;

            if (success)
            {
                MessageBox.Show("Test message sent! Check your Telegram.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Failed to send test message. Check your internet connection.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var settings = new TelegramSettings
            {
                ChatId = _linkedChatId ?? "",
                IsEnabled = chkEnable.IsChecked ?? false
            };
            settings.Save();
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            MemoryOptimizer.Trim();
        }
    }
}
