using BepInEx;
using BepInEx.Configuration;

// ReSharper disable InconsistentNaming

namespace GroundReset.Config;

public partial class ConfigsContainer
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
    private static BaseUnityPlugin Plugin = null!;
    private static Action? OnConfigurationChanged;
    private static DateTime LastConfigChange = DateTime.MinValue;

    public static void InitializeConfiguration(BaseUnityPlugin plugin)
    {
        Plugin = plugin;
        plugin.Config.SaveOnConfigSet = false;
        SetupWatcher();
        plugin.Config.ConfigReloaded += (_, _) => UpdateConfiguration();
        Instance = new ConfigsContainer();
        plugin.Config.SaveOnConfigSet = true;
        plugin.Config.Save();

        OnConfigurationChanged += () =>
        {
            Log.Info("Configuration Received");

            if(DateTime.Now - LastConfigUpdateTime < TimeSpan.FromSeconds(1)) return;
            LastConfigUpdateTime = DateTime.Now;

            Instance.ApplyConfiguration();

            Log.Info("Configuration applied");
        };

        IsInitialized = true;
    }

    private static void SetupWatcher()
    {
        FileSystemWatcher fileSystemWatcher = new(Paths.ConfigPath, Plugin.Info.Metadata.GUID);
        fileSystemWatcher.Changed += ConfigChanged;
        fileSystemWatcher.IncludeSubdirectories = true;
        fileSystemWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        fileSystemWatcher.EnableRaisingEvents = true;
    }

    private static void ConfigChanged(object sender, FileSystemEventArgs e)
    {
        if ((DateTime.Now - LastConfigChange).TotalSeconds <= 2) return;
        LastConfigChange = DateTime.Now;

        try
        {
            Plugin.Config.Reload();
        }
        catch
        {
            Log.Error("Unable reload config");
        }
    }

    private static void UpdateConfiguration()
    {
        try
        {
            OnConfigurationChanged?.Invoke();
        }
        catch (Exception e)
        {
            Log.Error($"Configuration error: {e.Message}", false);
        }
    }


    public static ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description)
    {
        var configEntry = Plugin.Config.Bind(group, name, value, description);
        return configEntry;
    }

    public static ConfigEntry<T> config<T>(string group, string name, T value, string description) =>
        config(group, name, value, new ConfigDescription(description));
}