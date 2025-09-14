namespace GroundReset.Compatibility.WardIsLove;

public static class WardMonoscriptExt
{
    public static Type ClassType() { return Type.GetType("WardIsLove.Extensions.WardMonoscriptExt, WardIsLove"); }

    public static WardMonoscript GetWardMonoscript(Vector3 pos)
    {
        var script =
            ModCompat.InvokeMethod<object>(ClassType(), null, "GetWardMonoscript", [pos]);
        return new WardMonoscript(script);
    }

    public static float GetWardRadius(this WardMonoscript wrapper)
    {
        return ModCompat.InvokeMethod<float>(ClassType(), null, "GetWardRadius", [wrapper.targetScript]);
    }

    public static bool GetDoorInteractOn(this WardMonoscript wrapper)
    {
        return ModCompat.InvokeMethod<bool>(ClassType(), null, "GetDoorInteractOn", [wrapper.targetScript]);
    }
}