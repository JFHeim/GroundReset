namespace GroundReset.Compatibility.WardIsLove;

public class CustomCheck : ModCompat
{
    public static Type ClassType() =>
        Type.GetType("WardIsLove.Util.CustomCheck, WardIsLove")
        ?? throw new Exception("Looks like WardIsLove is not installed or its API changed");

    public static bool CheckAccess(long playerID, Vector3 point, float radius = 0f, bool flash = true) =>
        InvokeMethod<bool>(ClassType(), null, "CheckAccess", [playerID, point, radius, flash]);
}