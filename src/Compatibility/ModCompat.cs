namespace GroundReset.Compatibility;

public class ModCompat
{
    public static T? InvokeMethod<T>(Type type, object? instance, string methodName, object[] parameter) =>
        (T?)type.GetMethod(methodName)?.Invoke(instance, parameter);

    public static T? GetField<T>(Type type, object? instance, string fieldName) =>
        (T?)type.GetField(fieldName)?.GetValue(instance);
}