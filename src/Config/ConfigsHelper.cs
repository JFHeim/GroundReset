using BepInEx;
using BepInEx.Configuration;

// ReSharper disable InconsistentNaming

namespace GroundReset.Config;

public partial class ConfigsContainer
{
    private static ConfigsContainer Instance {
        get => _isInitialized ? field : throw new NullReferenceException("ConfigsContainer is not yet initialized");
        set;
    } = null!;
    private static bool _isInitialized = false;
    private static BaseUnityPlugin _plugin = null!;
    private static Action? OnConfigurationChanged;
    private static DateTime _lastConfigChange = DateTime.MinValue;
    private static readonly HashSet<string> _configFilesToWatch = [];

    public static void InitializeConfiguration(BaseUnityPlugin plugin)
    {
        if (_isInitialized)
        {
            Log.Error("ConfigsContainer is already initialized");
            return;
        }

        _plugin = plugin;
        plugin.Config.SaveOnConfigSet = false;
        Instance = new ConfigsContainer();
        _configFilesToWatch.Add($"{_plugin.Info.Metadata.GUID}.cfg");
        SetupWatcher();
        plugin.Config.SaveOnConfigSet = true;
        plugin.Config.ConfigReloaded += (_, _) => UpdateConfiguration();
        plugin.Config.Save();

        OnConfigurationChanged += () =>
        {
            Log.Info("Configuration Received");
            Instance.ApplyConfiguration();
            Log.Info("Configuration applied");
        };

        _isInitialized = true;
    }

    private static void SetupWatcher()
    {
        foreach (var fileName in _configFilesToWatch.Where(x=> !string.IsNullOrEmpty(x)))
        {
            FileSystemWatcher fileSystemWatcher = new(Paths.ConfigPath, fileName);
            fileSystemWatcher.Changed += ConfigChanged;
            fileSystemWatcher.Created += ConfigChanged;
            fileSystemWatcher.IncludeSubdirectories = true;
            fileSystemWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            fileSystemWatcher.EnableRaisingEvents = true;
        }
    }

    private static void ConfigChanged(object sender, FileSystemEventArgs e)
    {
        if ((DateTime.Now - _lastConfigChange).TotalSeconds <= 2) return;
        _lastConfigChange = DateTime.Now;

        try
        {
            _plugin.Config.Reload();
        }
        catch
        {
            Log.Error("Unable reload config");
        }
    }

    private static void UpdateConfiguration()
    {
        try { OnConfigurationChanged?.Invoke(); }
        catch (Exception e) { Log.Error(e, "Configuration error", false); }
    }

    // ReSharper disable once InconsistentNaming
    public static ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description)
    {
        var configEntry = _plugin.Config.Bind(group, name, value, description);

        return configEntry;
    }

    // ReSharper disable once InconsistentNaming
    public static ConfigEntry<T> config<T>(string group, string name, T value, string description) =>
        config(group, name, value, new ConfigDescription(description));
}