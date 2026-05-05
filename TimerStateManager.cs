using System;
using System.Windows.Threading;

namespace atc
{
    public enum TimerMode
    {
        Clock,
        Alarm,
        Timer,
        Stopwatch
    }

    public class TimerStateManager
    {
        public event Action<string, string>? TimeUpdated;
        public event Action<bool>? BlinkStateChanged;
        public event Action<string>? NoteTextChanged;

        private DispatcherTimer _mainTimer;
        private DispatcherTimer _blinkTimer;

        private TimerMode _currentMode = TimerMode.Clock;
        public TimerMode CurrentMode => _currentMode;
        public string NoteText { get; private set; } = "";

        private string _lastTimeString = string.Empty;
        private string _lastNoteText = string.Empty;

        public void SetNoteText(string note)
        {
            if (NoteText == note) return;
            NoteText = note;
            NoteTextChanged?.Invoke(NoteText);
        }

        private DateTime _targetTime;
        private DateTime _timerEndTime;
        private DateTime _stopwatchStartTime;
        
        private bool _isZeroState = false;
        private bool _isBlinkVisible = true;

        public TimerStateManager()
        {
            _mainTimer = new DispatcherTimer();
            _mainTimer.Interval = TimeSpan.FromSeconds(1);
            _mainTimer.Tick += MainTimer_Tick!;

            _blinkTimer = new DispatcherTimer();
            _blinkTimer.Interval = TimeSpan.FromSeconds(1);
            _blinkTimer.Tick += BlinkTimer_Tick!;
        }

        public void Start()
        {
            _mainTimer.Start();
            UpdateDisplay();
        }

        /// <summary>
        /// Stops all timers and cleans up. Call when the window is closing.
        /// </summary>
        public void Stop()
        {
            _mainTimer.Stop();
            _blinkTimer.Stop();
        }

        public void SetMode(TimerMode mode, int hh, int mm, int ss, string note)
        {
            _currentMode = mode;
            NoteText = note;
            _isZeroState = false;
            _blinkTimer.Stop();
            _isBlinkVisible = true;
            BlinkStateChanged?.Invoke(true);

            if (mode == TimerMode.Alarm)
            {
                DateTime now = DateTime.Now;
                DateTime target = new DateTime(now.Year, now.Month, now.Day, hh, mm, ss);
                if (target <= now)
                {
                    target = target.AddDays(1);
                }
                _targetTime = target;
            }
            else if (mode == TimerMode.Timer)
            {
                // Timestamp-based: eliminates drift from DispatcherTimer jitter
                _timerEndTime = DateTime.Now.Add(new TimeSpan(hh, mm, ss));
            }
            else if (mode == TimerMode.Stopwatch)
            {
                _stopwatchStartTime = DateTime.Now;
            }

            UpdateDisplay();
        }

        /// <summary>
        /// Calculates the current TimeSpan for the active mode.
        /// Returns null for Clock mode (which formats from DateTime.Now directly).
        /// </summary>
        private TimeSpan? GetCurrentTimeSpan()
        {
            return _currentMode switch
            {
                TimerMode.Alarm     => _targetTime - DateTime.Now,
                TimerMode.Timer     => _timerEndTime - DateTime.Now,
                TimerMode.Stopwatch => DateTime.Now - _stopwatchStartTime,
                _                   => null
            };
        }

        private void MainTimer_Tick(object sender, EventArgs e)
        {
            if (_isZeroState) return;

            if (_currentMode == TimerMode.Clock)
            {
                UpdateDisplay();
                return;
            }

            TimeSpan ts = GetCurrentTimeSpan()!.Value;

            if ((_currentMode == TimerMode.Alarm || _currentMode == TimerMode.Timer) && ts.TotalSeconds <= 0)
                TriggerZeroState();
            else
                UpdateDisplay(ts);
        }

        private void BlinkTimer_Tick(object sender, EventArgs e)
        {
            _isBlinkVisible = !_isBlinkVisible;
            BlinkStateChanged?.Invoke(_isBlinkVisible);
        }

        private void TriggerZeroState()
        {
            _isZeroState = true;
            TimeUpdated?.Invoke("TIME", NoteText);
            // Instead of blinking, we just signal that zero state is reached
            // The blinking behavior is now handled in MainWindow.xaml.cs
            BlinkStateChanged?.Invoke(false); // Signal that we're in zero state (not blinking)
            _ = TelegramService.SendZeroNotificationAsync(NoteText);
        }

        /// <summary>
        /// Pure formatting method — can be called from tests with any mode/time.
        /// </summary>
        public static string FormatTime(TimerMode mode, TimeSpan? overrideTime = null)
        {
            if (mode == TimerMode.Clock)
            {
                DateTime now = DateTime.Now;
                return now.ToString(now.Hour < 10 ? "h:mm" : "HH:mm");
            }
            else
            {
                TimeSpan ts = overrideTime ?? TimeSpan.Zero;
                int totalMinutes = (mode == TimerMode.Stopwatch)
                    ? (int)Math.Floor(ts.TotalMinutes)
                    : (int)Math.Ceiling(ts.TotalMinutes);
                int h = totalMinutes / 60;
                int m = totalMinutes % 60;
                
                if (h > 0)
                {
                    if (h < 10)
                        return $"{h}:{m:00}";
                    else
                        return $"{h:00}:{m:00}";
                }
                else
                {
                    return $"{m}";
                }
            }
        }

        private void UpdateDisplay(TimeSpan? overrideTime = null)
        {
            TimeSpan? ts = overrideTime ?? GetCurrentTimeSpan();
            string timeString = FormatTime(_currentMode, ts);
            if (timeString != _lastTimeString || NoteText != _lastNoteText)
            {
                _lastTimeString = timeString;
                _lastNoteText = NoteText;
                TimeUpdated?.Invoke(timeString, NoteText);
            }
        }
    }
}
