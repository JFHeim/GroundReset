using BepInEx;

namespace GroundReset;

public static class Consts
{
    public const string TimerId = "JF_GroundReset";
    public const string MainSceneName = "main";
    public const string ThorwardPrefabName = "Thorward";
    public const string ArcaneWardPrefabName = "ArcaneWard";
    
    public const string ArcaneWardZdoKey = "Radius";
    public const string TerrCompPrefabName = "_TerrainCompiler";
    public static readonly int RadiusNetKey = "wardRadius".GetStableHashCode();
    
    public const string TimerPassedTimeSaveFileName = $"{nameof(ModName)}_LastTimerTimePassed.txt";

    public static readonly Lazy<string> TimerPassedTimeSaveFilePath = new(() => 
        Path.Combine(Paths.ConfigPath, TimerPassedTimeSaveFileName));
}