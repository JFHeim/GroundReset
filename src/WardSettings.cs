namespace GroundReset;

public readonly struct WardSettings(string prefabName, float radius)
{
    public readonly string PrefabName = prefabName;
    public readonly float Radius = radius;
    public readonly bool DynamicRadius = false;
    public readonly Func<ZDO, float>? GetDynamicRadius = null;

    public WardSettings(string prefabName, Func<ZDO, float> getDynamicRadius) : this(prefabName, 0)
    {
        DynamicRadius = true;
        GetDynamicRadius = getDynamicRadius;
    }
}