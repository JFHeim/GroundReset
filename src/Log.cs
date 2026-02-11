using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using GroundReset.Config;

namespace GroundReset;

public static class Log
{
    private static ManualLogSource? Logger;
    private static BaseUnityPlugin Plugin = null!;

    public static void InitializeConfiguration(BaseUnityPlugin plugin)
    {
        Plugin = plugin;
        Logger = GetProtectedLogger(Plugin);
    }

    private static ManualLogSource GetProtectedLogger(object instance)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        var type = instance.GetType();

        var prop = type.GetProperty("Logger",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
        );

        if (prop == null) throw new MissingMemberException(type.FullName, "Logger");

        var getter = prop.GetGetMethod(nonPublic: true);
        if (getter == null) throw new MissingMethodException($"Property {type.FullName}.Logger does not have a getter");

        var logSource = (ManualLogSource?)getter.Invoke(instance, null);
        return logSource ?? throw new Exception("Failed to get Logger");
    }

    public static void Info(string message, bool insertTimestamp = false)
    {
        if(Logger is null) return;

        if (insertTimestamp) message = DateTime.Now.ToString("G") + message;
        Logger.LogInfo(message);
    }

    public static void Error(Exception ex, string message = "", bool insertTimestamp = false)
    {
        if(Logger is null) return;

        if (insertTimestamp) message = DateTime.Now.ToString("G") + message;
        if (string.IsNullOrEmpty(message)) message += Environment.NewLine;

        Logger.LogError(message + ex);
    }

    public static void Error(string message, bool insertTimestamp = false)
    {
        if(Logger is null) return;

        if (insertTimestamp) message = DateTime.Now.ToString("G") + message;
        Logger.LogError(message);
    }

    public static void Warning(Exception ex, string message, bool insertTimestamp = false)
    {
        if(Logger is null) return;

        if (insertTimestamp) message = DateTime.Now.ToString("G") + message;
        if (string.IsNullOrEmpty(message)) message += Environment.NewLine;

        Logger.LogWarning(message + ex);
    }

    public static void Warning(string message, bool insertTimestamp = false)
    {
        if(Logger is null) return;

        if (insertTimestamp) message = DateTime.Now.ToString("G") + message;
        Logger.LogWarning(message);
    }
}