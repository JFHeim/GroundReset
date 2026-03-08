using System.Reflection;
using BepInEx;
using BepInEx.Logging;

namespace GroundReset;

public static class Log
{
    private static ManualLogSource? _logger;
    private static BaseUnityPlugin _plugin = null!;

    public static void InitializeConfiguration(BaseUnityPlugin plugin)
    {
        _plugin = plugin;
        _logger = GetProtectedLogger(_plugin);
    }

    private static ManualLogSource GetProtectedLogger(object instance)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        var type = instance.GetType();

        var prop = type.GetProperty("Logger", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        if (prop == null) throw new MissingMemberException(type.FullName, "Logger");

        var getter = prop.GetGetMethod(nonPublic: true);
        if (getter == null) throw new MissingMethodException($"Property {type.FullName}.Logger does not have a getter");

        var logSource = (ManualLogSource?)getter.Invoke(instance, null);
        return logSource ?? throw new Exception("Failed to get Logger");
    }

    public static void Info(string message, bool insertTimestamp = false)
    {
        if(_logger is null) return;

        if (insertTimestamp) message = DateTime.Now.ToString("G") + message;
        _logger.LogInfo(message);
    }

    public static void Error(Exception ex, string message = "", bool insertTimestamp = false)
    {
        if(_logger is null) return;

        if (insertTimestamp) message = DateTime.Now.ToString("G") + message;
        if (string.IsNullOrEmpty(message)) message += Environment.NewLine;

        _logger.LogError(message + ex);
    }

    public static void Error(string message, bool insertTimestamp = false)
    {
        if(_logger is null) return;

        if (insertTimestamp) message = DateTime.Now.ToString("G") + message;
        _logger.LogError(message);
    }

    public static void Warning(Exception ex, string message, bool insertTimestamp = false)
    {
        if(_logger is null) return;

        if (insertTimestamp) message = DateTime.Now.ToString("G") + message;
        if (string.IsNullOrEmpty(message)) message += Environment.NewLine;

        _logger.LogWarning(message + ex);
    }

    public static void Warning(string message, bool insertTimestamp = false)
    {
        if(_logger is null) return;

        if (insertTimestamp) message = DateTime.Now.ToString("G") + message;
        _logger.LogWarning(message);
    }
}