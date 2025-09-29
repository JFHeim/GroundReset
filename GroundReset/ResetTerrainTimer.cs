using System.Globalization;
using CodeMonkey;

namespace GroundReset;

public static class ResetTerrainTimer
{
    private static FunctionTimer? Timer { get; set; } = null;

    private static TimeSpan LastTimerTimePassed = TimeSpan.Zero;

    // private static ResetProcessState _resetProcessState = ResetProcessState.NotRunning;

    private static readonly Action? _onTimer = async void () =>
    {
        try
        {
            LogInfo("Timer Triggered, starting chunks reset", insertTimestamp:true);
            await Reseter.ResetAll();
            LogInfo("Timer Triggered, chunks have been reset, restarting the timer", insertTimestamp:true);
            RestartTimer();
        }
        catch (Exception exception1)
        {
            LogError($"OnTimer event failed with exception: {exception1}"); 
            RestartTimer();
        }
    };

    public static void RestartTimer()
    {
        try
        {
            LogInfo($"{nameof(ResetTerrainTimer)}.{nameof(RestartTimer)}");
            if (Helper.IsMainScene() == false) return;
            if (Helper.IsServer(true) == false) return;

            LogInfo("Stopping existing timers");
            FunctionTimer.StopAllTimersWithName(Consts.TimerId);
            Timer = null;
            
            var timerInterval = TimeSpan.FromMinutes(ConfigsContainer.TriggerIntervalInMinutes);
            if (LastTimerTimePassed != TimeSpan.Zero)
            {
                try { timerInterval -= LastTimerTimePassed; }
                catch { timerInterval = TimeSpan.FromSeconds(1);}

                if(timerInterval.TotalSeconds <= 0) timerInterval = TimeSpan.FromSeconds(1);
            }
            
            LogInfo($@"Creating new timer for {timerInterval:hh\:mm\:ss}", insertTimestamp:true);

            try
            {
                Timer = FunctionTimer.Create(
                    action: _onTimer, 
                    timer: (float)timerInterval.TotalSeconds, 
                    functionName: Consts.TimerId, 
                    useUnscaledDeltaTime: true,
                    stopAllWithSameName: true);
            }
            catch (Exception e)
            {
                LogError($"FunctionTimer.Create failed with exception: {e}");
            }
            LastTimerTimePassed = TimeSpan.Zero;
        }
        catch (Exception exception)
        {
            LogError($"{nameof(RestartTimer)} failed with exception: {exception}");
        }
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
        LogInfo($@"Loaded last timer passed time: {LastTimerTimePassed:hh\:mm\:ss}");
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
        File.WriteAllText(timerPassedTimeSaveFilePath,
            timerPassedTimeOnSeconds.ToString(NumberFormatInfo.InvariantInfo));

        LogInfo($@"Saved timer passed time to file: {LastTimerTimePassed:hh\:mm\:ss}");
    }

    // private enum ResetProcessState
    // {
    //     NotRunning,
    //     Running,
    // }
}

