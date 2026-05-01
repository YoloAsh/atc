using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace timer_project
{
    public partial class SettingsWindow : Window
    {
        private readonly TimerStateManager _timerState;
        private static readonly Regex _numberRegex = new Regex("[^0-9]+", RegexOptions.Compiled);

        public SettingsWindow(TimerStateManager timerState)
        {
            InitializeComponent();
            _timerState = timerState;
            LoadCurrentState();
        }

        private void LoadCurrentState()
        {
            txtNote.Text = _timerState.NoteText;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = _numberRegex.IsMatch(e.Text);
        }

        private int ParseInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return 0;
            if (int.TryParse(input, out int result)) return result;
            return 0;
        }

        private void ClockBtn_Click(object sender, RoutedEventArgs e)
        {
            _timerState.SetMode(TimerMode.Clock, 0, 0, 0, txtNote.Text);
        }

        private void AlarmBtn_Click(object sender, RoutedEventArgs e)
        {
            int hh = ParseInput(txtHH.Text);
            int mm = ParseInput(txtMM.Text);
            int ss = ParseInput(txtSS.Text);

            if (hh < 0 || hh > 23)
            {
                ShowError("Hours must be 0–23 for Alarm");
                return;
            }
            if (mm < 0 || mm > 59)
            {
                ShowError("Minutes must be 0–59");
                return;
            }
            if (ss < 0 || ss > 59)
            {
                ShowError("Seconds must be 0–59");
                return;
            }

            _timerState.SetMode(TimerMode.Alarm, hh, mm, ss, txtNote.Text);
        }

        private void TimerBtn_Click(object sender, RoutedEventArgs e)
        {
            int hh = ParseInput(txtHH.Text);
            int mm = ParseInput(txtMM.Text);
            int ss = ParseInput(txtSS.Text);

            if (hh < 0 || hh > 99)
            {
                ShowError("Hours must be 0–99 for Timer");
                return;
            }
            if (mm < 0 || mm > 59)
            {
                ShowError("Minutes must be 0–59");
                return;
            }
            if (ss < 0 || ss > 59)
            {
                ShowError("Seconds must be 0–59");
                return;
            }
            if (hh == 0 && mm == 0 && ss == 0)
            {
                // Hidden stopwatch mode
                _timerState.SetMode(TimerMode.Stopwatch, 0, 0, 0, txtNote.Text);
                return;
            }

            _timerState.SetMode(TimerMode.Timer, hh, mm, ss, txtNote.Text);
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void txtNote_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            _timerState.SetNoteText(txtNote.Text);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            MemoryOptimizer.Trim();
        }
    }
}
