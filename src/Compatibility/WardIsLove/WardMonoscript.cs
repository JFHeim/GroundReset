namespace GroundReset.Compatibility.WardIsLove;

public class WardMonoscript : ModCompat
{
    public object targetScript;

    public WardMonoscript(object targetScript) { this.targetScript = targetScript; }

    public static Type ClassType() { return Type.GetType("WardIsLove.Util.WardMonoscript, WardIsLove"); }

    public static bool CheckInWardMonoscript(Vector3 point, bool flash = false)
    {
        return InvokeMethod<bool>(ClassType(), null, "CheckInWardMonoscript", [point, flash]);
    }

    public static bool CheckAccess(Vector3 point, float radius = 0.0f, bool flash = true, bool wardCheck = false)
    {
        return InvokeMethod<bool>(ClassType(), null, "CheckAccess",
            [point, radius, flash, wardCheck]);
    }

    public ZNetView GetZNetView()
    {
        if (targetScript == null) return null;

        return GetField<ZNetView>(ClassType(), targetScript, "m_nview");
    }

    public ZDO GetZDO()
    {
        var netView = GetZNetView();
        return netView != null && netView && netView.IsValid() ? netView.GetZDO() : null;
    }
}