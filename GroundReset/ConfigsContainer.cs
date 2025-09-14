using System.Globalization;
using BepInEx.Configuration;
using GroundReset.Patch;

namespace GroundReset;

public class ConfigsContainer
{
    public static ConfigsContainer Instance
    {
        get
        {
            System.Diagnostics.Debug.Assert(IsInitialized == true);
            return field;
        }
        private set;
    } = null!;
    private static bool IsInitialized = false;
    private static DateTime LastConfigUpdateTime = DateTime.MinValue;

    public static float TriggerIntervalInMinutes => Instance._triggerIntervalInMinutesConfig.Value;
    // public static float SavedTimeUpdateInterval => Instance._savedTimeUpdateIntervalConfig.Value;
    public static float Divider => Instance._dividerConfig.Value;
    public static float MinHeightToSteppedReset => Instance._minHeightToSteppedResetConfig.Value;
    public static float PaintsCompareTolerance => Instance._paintsCompareToleranceConfig.Value;
    public static List<Color> PaintsToIgnore => Instance._paintsToIgnore;
    public static bool ResetSmoothing => Instance._resetSmoothingConfig.Value;
    // public static bool ResetSmoothingLast => Instance._resetSmoothingLastConfig.Value;
    public static bool ResetPaintResetLastly => Instance._resetPaintResetLastlyConfig.Value;
    
    private readonly ConfigEntry<float>  _triggerIntervalInMinutesConfig;
    // private readonly ConfigEntry<float>  _savedTimeUpdateIntervalConfig;
    private readonly ConfigEntry<float>  _dividerConfig;
    private readonly ConfigEntry<float>  _minHeightToSteppedResetConfig;
    private readonly ConfigEntry<float>  _paintsCompareToleranceConfig;
    private readonly ConfigEntry<string> _paintsToIgnoreConfig;
    private readonly ConfigEntry<bool>   _resetSmoothingConfig;
    // private readonly ConfigEntry<bool>   _resetSmoothingLastConfig;
    private readonly ConfigEntry<bool>   _resetPaintResetLastlyConfig;
    
    private float _lastTriggerIntervalInMinutes = -1;
    private readonly List<Color> _paintsToIgnore = [];
    
    private readonly Dictionary<string, Color> vanillaPresets = new()
    {
        { "Dirt", Heightmap.m_paintMaskDirt },
        { "Cultivated", Heightmap.m_paintMaskCultivated },
        { "Paved", Heightmap.m_paintMaskPaved },
        { "Nothing", Heightmap.m_paintMaskNothing }
    };

    private ConfigsContainer()
    {
        _triggerIntervalInMinutesConfig   = config("General",      "TheTriggerTime",                           4320f,                   "Time in real minutes between reset steps.");
        _dividerConfig                    = config("General",      "Divider",                                  1.7f,                    "The divider for the terrain restoration. Current value will be divided by this value. Learn more on mod page.");
        _minHeightToSteppedResetConfig    = config("General",      "Min Height To Stepped Reset",              0.2f,                    "If the height delta is lower than this value, it will be counted as zero.");
        // _savedTimeUpdateIntervalConfig    = config("General",      "SavedTime Update Interval (seconds)",      120f,                    "How often elapsed time will be saved to config file.");
        _paintsToIgnoreConfig             = config("General",      "Paint To Ignore",                          "(Paved), (Cultivated)", $"This paints will be ignored in the reset process.\n{vanillaPresets.Keys.GetString()}");
        _paintsCompareToleranceConfig     = config("General",      "Paints Compair Tolerance",                 0.3f,                    "The accuracy of the comparison of colors. Since the current values of the same paint may differ from the reference in different situations, they have to be compared with the difference in this value.");
        _resetSmoothingConfig             = config("General",      "Reset Smoothing",                          true,                    "Should the terrain smoothing be reset");
        _resetPaintResetLastlyConfig      = config("General",      "Process Paint Lastly",                     true,                    "Set to true so that the paint is reset only after the ground height delta and smoothing is completely reset. Otherwise, the paint will be reset at each reset step along with the height delta.");
        // _resetSmoothingLastConfig         = config("General",      "Process Smoothing After Height",           true,                    "Set to true so that the smoothing is reset only after the ground height delta is completely reset. Otherwise, the smoothing will be reset at each reset step along with the height delta.");
        // _debugConfig                   = config("Debug",                    "Do some test debugs",                      false,                   "");
        // _debugTestConfig               = config("Debug",                    "Do some dev goofy debugs",                 false,                   "");
        // _debugPaintArrayMismatchConfig = config("Debug",                    "Debug Paint Array Missmatch",              true,                    "Should mod notify if the number of colors in the paint array does not match the number of colors in the paint mask.Yes, that is an error, but idk what to with it");

        IsInitialized = true;
        
        OnConfigurationChanged += UpdateConfiguration;
    }

    public static void InitializeConfiguration() => Instance = new ConfigsContainer();

    private void UpdateConfiguration()
    {
        if(DateTime.Now - LastConfigUpdateTime < TimeSpan.FromSeconds(1)) return;

        var diff = Math.Abs(_lastTriggerIntervalInMinutes - TriggerIntervalInMinutes);
        var isMainScene = Helper.IsMainScene();
        
        LogInfo($"diff={diff:F00}, isMainScene={isMainScene}");
        
        if (diff > 1f && isMainScene)
        {
            ResetTerrainTimer.RestartTimer();
        }
        _lastTriggerIntervalInMinutes = TriggerIntervalInMinutes;

        ResetTerrainTimer.LoadTimePassedFromFile();
        ParsePaints(_paintsToIgnoreConfig.Value);
        
        if (ZNetScene.instance) InitWardsSettings.RegisterWards();
        LogInfo($"PaintsToIgnore = {PaintsToIgnore.GetString()}");
        
        LogInfo("Configuration Received");
    }

    private void ParsePaints(string str)
    {
        PaintsToIgnore.Clear();
        var pairs = str.Split(["), ("], StringSplitOptions.RemoveEmptyEntries);
        PaintsToIgnore.Capacity = pairs.Length;

        foreach (var pair in pairs)
        {
            var trimmedPair = pair.Trim('(', ')');
            if (vanillaPresets.TryGetValue(trimmedPair.Replace(" ", ""), out var color))
            {
                PaintsToIgnore.Add(color);
                continue;
            }

            var keyValue = trimmedPair.Split([", "], StringSplitOptions.RemoveEmptyEntries);

            if (keyValue.Length != 4)
            {
                LogError($"Could not parse color: '{keyValue.GetString()}', expected format: (r, b, g, alpha)\n" + vanillaPresets.Keys.GetString());
                continue;
            }

            var aStr = keyValue[0];
            var bStr = keyValue[1];
            var gStr = keyValue[2];
            var alphaStr = keyValue[3];

            if (!float.TryParse(aStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var a))
            {
                LogError($"Could not parse a value: '{aStr}'");
                continue;
            }

            if (!float.TryParse(bStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var b))
            {
                LogError($"Could not parse b value: '{bStr}'");
                continue;
            }

            if (!float.TryParse(gStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var g))
            {
                LogError($"Could not parse g value: '{gStr}'");
                continue;
            }

            if (!float.TryParse(alphaStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var alpha))
            {
                LogError($"Could not parse alpha value: '{alphaStr}'");
                continue;
            }

            color = new Color(a, b, g, alpha);
            PaintsToIgnore.Add(color);
        }
    }
}