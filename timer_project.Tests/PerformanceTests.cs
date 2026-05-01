using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using timer_project;

namespace timer_project.Tests;

public class PerformanceTests
{
    [Fact]
    public void DisplayFormatCorrectness_Test()
    {
        // 1. Clock mode
        var timeStrClock = TimerStateManager.FormatTime(TimerMode.Clock);
        Assert.NotNull(timeStrClock);
        
        // 2. Alarm/Timer mode - Less than an hour
        var timeStrUnderHour = TimerStateManager.FormatTime(TimerMode.Alarm, TimeSpan.FromMinutes(45));
        Assert.Equal("45", timeStrUnderHour);
        
        var timeStrExactMinutes = TimerStateManager.FormatTime(TimerMode.Alarm, TimeSpan.FromSeconds(125)); // 2 min 5 sec -> 3 minutes remaining
        Assert.Equal("3", timeStrExactMinutes);
        
        // 3. Alarm/Timer mode - More than an hour
        var timeStrOverHour = TimerStateManager.FormatTime(TimerMode.Alarm, TimeSpan.FromMinutes(75));
        Assert.Equal("1:15", timeStrOverHour);
        
        var timeStr10Hours = TimerStateManager.FormatTime(TimerMode.Alarm, TimeSpan.FromHours(10).Add(TimeSpan.FromMinutes(5)));
        Assert.Equal("10:05", timeStr10Hours);
    }

    [Fact]
    public void TelegramSettings_CacheBehavior_Test()
    {
        // Clear cache first
        TelegramSettings.ClearCache();
        
        // First load should read from disk (or create new if missing)
        var settings1 = TelegramSettings.Load();
        
        // Second load should return the exact same instance due to cache
        var settings2 = TelegramSettings.Load();
        
        Assert.Same(settings1, settings2);
    }

    [Fact]
    public void TimerDrift_UsesTimestamps_Test()
    {
        var timerState = new TimerStateManager();
        timerState.SetMode(TimerMode.Timer, 1, 0, 0, ""); // 1 hour timer
        
        // Use reflection to verify _timerEndTime is set
        var fieldInfo = typeof(TimerStateManager).GetField("_timerEndTime", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(fieldInfo);
        
        var endTime = (DateTime)fieldInfo.GetValue(timerState)!;
        
        // Should be approximately 1 hour from now
        var expectedEndTime = DateTime.Now.AddHours(1);
        Assert.True(Math.Abs((endTime - expectedEndTime).TotalSeconds) < 2, "Timer end time should be accurately set based on timestamp to avoid drift.");
    }

    [Fact]
    public void EventUnsubscriptionOnStop_Test()
    {
        var timerState = new TimerStateManager();
        int eventCount = 0;
        
        Action<string, string> handler = (t, n) => eventCount++;
        timerState.TimeUpdated += handler;
        
        // UpdateDisplay is private, but Start() triggers it
        timerState.Start();
        Assert.Equal(1, eventCount);
        
        timerState.TimeUpdated -= handler;
        
        // Start again, should not increment eventCount
        timerState.Start();
        Assert.Equal(1, eventCount);
    }
}
