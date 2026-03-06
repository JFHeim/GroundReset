using BepInEx.Bootstrap;
using BepInEx.Configuration;

namespace GroundReset.Compatibility.WardIsLove;

public class WardIsLovePlugin : ModCompat
{
    private const string GUID = "Azumatt.WardIsLove";
    private static readonly System.Version _minVersion = new(2, 3, 3);

    public static Type ClassType() =>
        Type.GetType("WardIsLove.WardIsLovePlugin, WardIsLove")
        ?? throw new Exception("Looks like WardIsLove is not installed or its API changed");

    public static bool IsLoaded() =>
        Chainloader.PluginInfos.ContainsKey(GUID) &&
        Chainloader.PluginInfos[GUID].Metadata.Version >= _minVersion;

    public static ConfigEntry<bool> WardEnabled() =>
        GetField<ConfigEntry<bool>>(ClassType(), null, "WardEnabled")
        ?? throw new Exception("Looks like WardIsLove is not installed or its API changed");

    public static ConfigEntry<float> WardRange() =>
        GetField<ConfigEntry<float>>(ClassType(), null, "WardRange")
        ?? throw new Exception("Looks like WardIsLove is not installed or its API changed");
}