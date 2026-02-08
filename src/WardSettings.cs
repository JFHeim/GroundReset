namespace GroundReset;

public struct WardSettings
{
    public string prefabName;
    public float radius;
    public bool dynamicRadius = false;
    public Func<ZDO, float> getDynamicRadius;

    public WardSettings(string prefabName, float radius)
    {
        this.prefabName = prefabName;
        this.radius = radius;
    }

    public WardSettings(string prefabName, Func<ZDO, float> getDynamicRadius) : this(prefabName, 0)
    {
        dynamicRadius = true;
        this.getDynamicRadius = getDynamicRadius;
    }
}