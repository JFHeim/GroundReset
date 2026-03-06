using BepInEx.Bootstrap;

namespace GroundReset.Compatibility.kgMarketplace;

public class MarketplaceTerritorySystem
{
    private const string GUID = "MarketplaceAndServerNPCs";

    public static bool IsLoaded() => Chainloader.PluginInfos.ContainsKey(GUID);

    public static bool PointInTerritory(Vector3 pos)
    {
        if (IsLoaded() == false) return false;
        var inTerritory = PointInTerritoryApi(pos) ?? false;
        return inTerritory;
    }
    
    private static bool? PointInTerritoryApi(Vector3 pos)
    {
        var territoryType = AccessTools.TypeByName("Marketplace.Modules.TerritorySystem.TerritorySystem_DataTypes.Territory");
        if (territoryType == null) return null;

        var getCurrentTerritoryMethod = AccessTools.Method(territoryType, "GetCurrentTerritory");
        if (getCurrentTerritoryMethod == null) return null;

        var territory = getCurrentTerritoryMethod.Invoke(null, [pos]);
        return territory != null;
    }
}