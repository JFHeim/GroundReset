using System.Globalization;
using CodeMonkey;
using GroundReset.Config;

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
            Log.Info("Timer Triggered, starting chunks reset", insertTimestamp:true);
            await Reseter.ResetAll();
            Log.Info("Timer Triggered, chunks have been reset, restarting the timer", insertTimestamp:true);
            RestartTimer();
        }
        catch (Exception exception1)
        {
            Log.Error($"OnTimer event failed with exception: {exception1}"); 
            RestartTimer();
        }
    };

    public static void RestartTimer()
    {
        try
        {
            Log.Info($"{nameof(ResetTerrainTimer)}.{nameof(RestartTimer)}");
            if (Helper.IsMainScene() == false) return;
            if (Helper.IsServer(true) == false) return;

            Log.Info("Stopping existing timers");
            FunctionTimer.StopAllTimersWithName(Consts.TimerId);
            Timer = null;
            
            var timerInterval = TimeSpan.FromMinutes(ConfigsContainer.TriggerIntervalInMinutes);
            if (LastTimerTimePassed != TimeSpan.Zero)
            {
                try { timerInterval -= LastTimerTimePassed; }
                catch { timerInterval = TimeSpan.FromSeconds(1);}

                if(timerInterval.TotalSeconds <= 0) timerInterval = TimeSpan.FromSeconds(1);
            }
            
            Log.Info($@"Creating new timer for {timerInterval:hh\:mm\:ss}", insertTimestamp:true);

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
                Log.Error($"FunctionTimer.Create failed with exception: {e}");
            }
            LastTimerTimePassed = TimeSpan.Zero;
        }
        catch (Exception exception)
        {
            Log.Error($"{nameof(RestartTimer)} failed with exception: {exception}");
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
            Log.Warning("Failed to read invalid value from timer save file, overwritten with zero");
            File.WriteAllText(timerPassedTimeSaveFilePath, 0f.ToString(NumberFormatInfo.InvariantInfo));
            LastTimerTimePassed = TimeSpan.Zero;
            return;
        }

        LastTimerTimePassed = TimeSpan.FromSeconds(value);
        Log.Info($@"Loaded last timer passed time: {LastTimerTimePassed:hh\:mm\:ss}");
    }

    public static void SavePassedTimerTimeToFile()
    {
        System.Diagnostics.Debug.Assert(Timer is not null);
        if (Timer is null)
        {
            Log.Warning("Can not save timer passed time before its creation");
            return;
        }
        
        var timerPassedTimeSaveFilePath = Consts.TimerPassedTimeSaveFilePath.Value;
        if (!File.Exists(timerPassedTimeSaveFilePath)) File.Create(timerPassedTimeSaveFilePath);
        
        var timerPassedTimeOnSeconds = Timer.Timer;
        LastTimerTimePassed = TimeSpan.FromSeconds(timerPassedTimeOnSeconds);
        File.WriteAllText(timerPassedTimeSaveFilePath,
            timerPassedTimeOnSeconds.ToString(NumberFormatInfo.InvariantInfo));

        Log.Info($@"Saved timer passed time to file: {LastTimerTimePassed:hh\:mm\:ss}");
    }

    // private enum ResetProcessState
    // {
    //     NotRunning,
    //     Running,
    // }
}

