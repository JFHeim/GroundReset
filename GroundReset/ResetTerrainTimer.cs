using System.Globalization;
using CodeMonkey;

namespace GroundReset;

public static class ResetTerrainTimer
{
    private static FunctionTimer? Timer { get; set; } = null;

    private static TimeSpan LastTimerTimePassed = TimeSpan.Zero;

    private static readonly Action? _onTimer = () =>
    {
        LogInfo("Timer Triggered, Resetting...");
        ResetAll();
        RestartTimer();
    };
    
    public static void RestartTimer()
    {
        LogDebug("ResetTerrainTimer.InitTimer");
        if (Helper.IsMainScene() == false) return;
        if (Helper.IsServer(true) == false) return;

        LogDebug("Stopping existing timers");
        FunctionTimer.StopAllTimersWithName(Consts.TimerId);
        Timer = null;
        
        var timerInterval = TimeSpan.FromMinutes(ConfigsContainer.TriggerIntervalInMinutes);
        if (LastTimerTimePassed != TimeSpan.Zero) timerInterval = LastTimerTimePassed;
        
        LogDebug($@"Creating new timer for {timerInterval:hh\:mm\:ss}");
        
        Timer = FunctionTimer.Create(
            action: _onTimer, 
            timer: (float)timerInterval.TotalSeconds, 
            functionName: Consts.TimerId, 
            useUnscaledDeltaTime: true,
            stopAllWithSameName: true);
        
        LastTimerTimePassed = TimeSpan.Zero;
    }

    public static void LoadTimePassedFromFile()
    {
        var timerPassedTimeSaveFilePath = Consts.TimerPassedTimeSaveFilePath.Value;
        if (!File.Exists(timerPassedTimeSaveFilePath))
        {
            File.Create(timerPassedTimeSaveFilePath);
            File.WriteAllText(timerPassedTimeSaveFilePath, 0f.ToString(NumberFormatInfo.InvariantInfo));
            LastTimerTimePassed = TimeSpan.Zero;
            return;
        }

        var readAllText = File.ReadAllText(timerPassedTimeSaveFilePath);
        if (!float.TryParse(readAllText, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out var value))
        {
            LogWarning("Failed to read invalid value from timer save file, overwritten with zero");
            File.WriteAllText(timerPassedTimeSaveFilePath, 0f.ToString(NumberFormatInfo.InvariantInfo));
            LastTimerTimePassed = TimeSpan.Zero;
            return;
        }

        LastTimerTimePassed = TimeSpan.FromSeconds(value);
        LogDebug($@"Loaded last timer passed time: {LastTimerTimePassed:hh\:mm\:ss}");
    }

    public static void SavePassedTimerTimeToFile()
    {
        System.Diagnostics.Debug.Assert(Timer is not null);
        if (Timer is null)
        {
            LogWarning("Can not save timer passed time before its creation");
            return;
        }
        
        var timerPassedTimeSaveFilePath = Consts.TimerPassedTimeSaveFilePath.Value;
        if (!File.Exists(timerPassedTimeSaveFilePath)) File.Create(timerPassedTimeSaveFilePath);
        
        var timerPassedTimeOnSeconds = Timer.Timer;
        LastTimerTimePassed = TimeSpan.FromSeconds(timerPassedTimeOnSeconds);
        File.WriteAllText(timerPassedTimeSaveFilePath, timerPassedTimeOnSeconds.ToString(NumberFormatInfo.InvariantInfo));
        
        LogDebug($@"Saved timer passed time to file: {LastTimerTimePassed:hh\:mm\:ss}");
    }
}