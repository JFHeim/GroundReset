using BepInEx.Bootstrap;

namespace GroundReset.Compatibility.kgMarketplace;

public class MarketplaceTerritorySystem
{
    private const string GUID = "MarketplaceAndServerNPCs";

    public static bool IsLoaded() { return Chainloader.PluginInfos.ContainsKey(GUID); }

    public static bool PointInTerritory(Vector3 pos)
    {
        if (IsLoaded() == false) return false;
        var inTerritory = MarketplaceTerritorySystem_RAW.PointInTerritory(pos);
        return inTerritory;
    }
}