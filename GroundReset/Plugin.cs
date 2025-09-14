using System.Globalization;
using BepInEx;
using BepInEx.Configuration;
using GroundReset.Patch;
using UnityEngine.SceneManagement;

namespace GroundReset;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class Plugin : BaseUnityPlugin
{
    private const string ModName = "GroundReset",
        ModAuthor = "Frogger",
        ModVersion = "2.6.0",
        ModGUID = $"com.{ModAuthor}.{ModName}";

    internal static Action onTimer;
    internal static FunctionTimer timer;
    private static string vanillaPresetsMsg;

    internal static ConfigEntry<float> timeInMinutesConfig;
    internal static ConfigEntry<float> timePassedInMinutesConfig;
    internal static ConfigEntry<float> savedTimeUpdateIntervalConfig;
    internal static ConfigEntry<float> dividerConfig;
    internal static ConfigEntry<float> minHeightToSteppedResetConfig;
    internal static ConfigEntry<float> paintsCompairToleranceConfig;
    internal static ConfigEntry<string> paintsToIgnoreConfig;
    internal static ConfigEntry<bool> resetSmoothingConfig;
    internal static ConfigEntry<bool> resetSmoothingLastConfig;
    internal static ConfigEntry<bool> resetPaintLastConfig;
    internal static ConfigEntry<bool> debugConfig;
    internal static ConfigEntry<bool> debugTestConfig;
    internal static ConfigEntry<bool> debugPaintArrayMissmatchConfig;
    internal static float timeInMinutes = -1;
    internal static float paintsCompairTolerance = 0.3f;
    internal static float timePassedInMinutes;
    internal static float savedTimeUpdateInterval;
    internal static bool debug;
    internal static bool debug_test;
    internal static bool debug_paintArrayMissmatch;
    internal static bool resetPaintLast;
    internal static List<Color> paintsToIgnore = new();
    private Dictionary<string, Color> vanillaPresets;

    private void Awake()
    {
        vanillaPresets = new Dictionary<string, Color>
        {
            { "Dirt", m_paintMaskDirt },
            { "Cultivated", m_paintMaskCultivated },
            { "Paved", m_paintMaskPaved },
            { "Nothing", m_paintMaskNothing }
        };
        vanillaPresetsMsg = "Vanilla presets: " + vanillaPresets.Keys.GetString();

        CreateMod(this, ModName, ModAuthor, ModVersion, ModGUID);
        OnConfigurationChanged += UpdateConfiguration;

        timeInMinutesConfig = config("General", "TheTriggerTime", 4320f, "Time in real minutes between reset steps.");
        dividerConfig = config("General", "Divider", 1.7f,
            "The divider for the terrain restoration. Current value will be divided by this value. Learn more on mod page.");
        minHeightToSteppedResetConfig = config("General", "Min Height To Stepped Reset", 0.2f,
            "If the height delta is lower than this value, it will be counted as zero.");
        savedTimeUpdateIntervalConfig = config("General", "SavedTime Update Interval (seconds)", 120f,
            "How often elapsed time will be saved to config file.");
        timePassedInMinutesConfig = config("DO NOT TOUCH", "time has passed since the last trigger", 0f,
            new ConfigDescription("DO NOT TOUCH this", null, new ConfigurationManagerAttributes { Browsable = false }));
        paintsToIgnoreConfig =
            config("General", "Paint To Ignore", "(Paved), (Cultivated)",
                $"This paints will be ignored in the reset process.\n{vanillaPresetsMsg}");
        paintsCompairToleranceConfig = config("General", "Paints Compair Tolerance", 0.3f,
            "The accuracy of the comparison of colors. "
            + "Since the current values of the same paint may differ from the reference in different situations, "
            + "they have to be compared with the difference in this value.");
        resetSmoothingConfig = config("General", "Reset Smoothing", true, "Should the terrain smoothing be reset");
        resetPaintLastConfig = config("General", "Process Paint Lastly", true,
            "Set to true so that the paint is reset only after the ground height delta and smoothing is completely reset. "
            + "Otherwise, the paint will be reset at each reset step along with the height delta.");
        resetSmoothingLastConfig = config("General", "Process Smoothing After Height", true,
            "Set to true so that the smoothing is reset only after the ground height delta is completely reset. "
            + "Otherwise, the smoothing will be reset at each reset step along with the height delta.");
        debugConfig = config("Debug", "Do some test debugs", false, "");
        debugTestConfig = config("Debug", "Do some dev goofy debugs", false, "");
        debugPaintArrayMissmatchConfig = config("Debug", "Debug Paint Array Missmatch", true, 
            "Should mod notify if the number of colors in the paint array does not match the number of colors in the paint mask."
            + "Yes, that is an error, but idk what to with it");

        onTimer += () =>
        {
            Debug("Timer Triggered, Resetting...");
            ResetAll();
            InitTimer();
        };
    }

    private void UpdateConfiguration()
    {
        if (Math.Abs(timeInMinutes - timeInMinutesConfig.Value) > 1f
            && SceneManager.GetActiveScene().name == "main") InitTimer();

        debug = debugConfig.Value;
        debug_test = debugTestConfig.Value;
        debug_paintArrayMissmatch = debugPaintArrayMissmatchConfig.Value;
        timeInMinutes = timeInMinutesConfig.Value;
        timePassedInMinutes = timePassedInMinutesConfig.Value;
        savedTimeUpdateInterval = savedTimeUpdateIntervalConfig.Value;
        paintsCompairTolerance = paintsCompairToleranceConfig.Value;
        resetPaintLast = resetPaintLastConfig.Value;
        TryParsePaints(paintsToIgnoreConfig.Value);
        
        if (ZNetScene.instance) InitWardsSettings.RegisterWards();

        if (debug) DebugWarning($"paintsToIgnore = {paintsToIgnore.GetString()}");

        Debug("Configuration Received");
    }

    private void TryParsePaints(string str)
    {
        paintsToIgnore.Clear();
        var pairs = str.Split(new[] { "), (" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var pair in pairs)
        {
            var trimmedPair = pair.Trim('(', ')');
            if (vanillaPresets.TryGetValue(trimmedPair.Replace(" ", ""), out var color))
            {
                paintsToIgnore.Add(color);
                continue;
            }

            var keyValue = trimmedPair.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);


            if (keyValue.Length != 4)
            {
                DebugError($"Could not parse color: '{keyValue.GetString()}', expected format: (r, b, g, alpha)\n"
                           + vanillaPresetsMsg);
                continue;
            }

            var a_str = keyValue[0];
            var b_str = keyValue[1];
            var g_str = keyValue[2];
            var alpha_str = keyValue[3];

            if (!float.TryParse(a_str, NumberStyles.Float, CultureInfo.InvariantCulture, out var a))
            {
                DebugError($"Could not parse a value: '{a_str}'");
                continue;
            }

            if (!float.TryParse(b_str, NumberStyles.Float, CultureInfo.InvariantCulture, out var b))
            {
                DebugError($"Could not parse b value: '{b_str}'");
                continue;
            }

            if (!float.TryParse(g_str, NumberStyles.Float, CultureInfo.InvariantCulture, out var g))
            {
                DebugError($"Could not parse g value: '{g_str}'");
                continue;
            }

            if (!float.TryParse(alpha_str, NumberStyles.Float, CultureInfo.InvariantCulture, out var alpha))
            {
                DebugError($"Could not parse alpha value: '{alpha_str}'");
                continue;
            }

            color = new Color(a, b, g, alpha);
            paintsToIgnore.Add(color);
        }
    }

    private static void InitTimer()
    {
        FunctionTimer.StopAllTimersWithName("JF_GroundReset");
        FunctionTimer.Create(onTimer, timeInMinutesConfig.Value * 60, "JF_GroundReset", true, true);
    }
}