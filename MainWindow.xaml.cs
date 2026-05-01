using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace timer_project
{
    public partial class MainWindow : Window
    {
        private const double LargeWidth = 1000, LargeHeight = 500;
        private const double SmallWidth = 140, SmallHeight = 75;
        private const double ScreenEdgeMargin = 20;

        private Brush[] colors = new Brush[] { Brushes.White, Brushes.Green, Brushes.Blue, Brushes.Yellow, Brushes.Red };
        private int currentColorIndex = 0;

        private double[] opacities = new double[] { 0.1, 0.25, 0.5, 0.75, 1.0 };
        private int currentOpacityIndex = 2; // Default 0.5

        private readonly TimerStateManager _timerState;

        public MainWindow()
        {
            InitializeComponent();
            _timerState = new TimerStateManager();
            this.Opacity = opacities[currentOpacityIndex];
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _timerState.TimeUpdated += OnTimeUpdated;
            _timerState.BlinkStateChanged += OnBlinkStateChanged;
            _timerState.NoteTextChanged += OnNoteTextChanged;
            _timerState.Start();
        }

        private void UpdateNoteDisplay(string noteText)
        {
            if (!string.IsNullOrWhiteSpace(noteText))
            {
                if (NoteTextBlock.Text != noteText)
                    NoteTextBlock.Text = noteText;
                NoteTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                NoteTextBlock.Visibility = Visibility.Hidden;
            }
        }

        private void OnNoteTextChanged(string noteText)
        {
            UpdateNoteDisplay(noteText);
        }

        private void OnTimeUpdated(string timeText, string noteText)
        {
            if (TimeTextBlock.Text != timeText)
                TimeTextBlock.Text = timeText;
            UpdateNoteDisplay(noteText);
        }

        private void OnBlinkStateChanged(bool isVisible)
        {
            TimeTextBlock.Visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
            NoteTextBlock.Visibility = (isVisible && !string.IsNullOrWhiteSpace(NoteTextBlock.Text)) ? Visibility.Visible : Visibility.Hidden;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                currentColorIndex = (currentColorIndex + 1) % colors.Length;
                TimeTextBlock.Foreground = colors[currentColorIndex];
                NoteTextBlock.Foreground = colors[currentColorIndex];
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                currentOpacityIndex = (currentOpacityIndex + 1) % opacities.Length;
                this.Opacity = opacities[currentOpacityIndex];
            }
        }

        private void PinMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;
            PinMenuItem.Header = this.Topmost ? "Unpin" : "Pin";
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow(_timerState);
            settings.Owner = this;
            settings.Show();
        }

        private void TelegramMenuItem_Click(object sender, RoutedEventArgs e)
        {
            TelegramSetupWindow telegramSetup = new TelegramSetupWindow();
            telegramSetup.Owner = this;
            telegramSetup.Show();
        }

        private void SizeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.Width == LargeWidth)
            {
                this.Width = SmallWidth;
                this.Height = SmallHeight;
                SizeMenuItem.Header = "Bigger";

                var workArea = SystemParameters.WorkArea;
                this.Left = workArea.Right - this.Width - ScreenEdgeMargin;
                this.Top = workArea.Top + ScreenEdgeMargin;
            }
            else
            {
                this.Width = LargeWidth;
                this.Height = LargeHeight;
                SizeMenuItem.Header = "Smaller";

                var workArea = SystemParameters.WorkArea;
                this.Left = workArea.Left + (workArea.Width - this.Width) / 2;
                this.Top = workArea.Top + (workArea.Height - this.Height) / 2;
            }
        }

        private void ResizeGripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ResizeGripMenuItem.IsChecked)
            {
                this.ResizeMode = ResizeMode.CanResizeWithGrip;
                WindowResizeGrip.Visibility = Visibility.Visible;
            }
            else
            {
                this.ResizeMode = ResizeMode.NoResize;
                WindowResizeGrip.Visibility = Visibility.Collapsed;
            }
        }

        private void BoldMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (BoldMenuItem.IsChecked)
            {
                TimeTextBlock.FontWeight = FontWeights.Bold;
                BoldMenuItem.Header = "Normal";
            }
            else
            {
                TimeTextBlock.FontWeight = FontWeights.Normal;
                BoldMenuItem.Header = "Bold";
            }
        }

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _timerState.TimeUpdated -= OnTimeUpdated;
            _timerState.BlinkStateChanged -= OnBlinkStateChanged;
            _timerState.NoteTextChanged -= OnNoteTextChanged;
            _timerState.Stop();
            base.OnClosed(e);
        }
    }
}