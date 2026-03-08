using System.Reflection;
using BepInEx.Bootstrap;

namespace GroundReset.Compatibility.kgMarketplace;

public static class MarketplaceTerritorySystem
{
    private const string MarketModGuid = "MarketplaceAndServerNPCs";

    private static readonly Lazy<MethodInfo?> GetCurrentTerritoryMethod = new(() =>
    {
        var territoryType = AccessTools.TypeByName("Marketplace.Modules.TerritorySystem.TerritorySystem_DataTypes+Territory");
        if (territoryType == null) return null;
        return AccessTools.Method(territoryType, "GetCurrentTerritory");
    });

    public static bool IsLoaded() => Chainloader.PluginInfos.ContainsKey(MarketModGuid);

    public static bool PointInTerritory(Vector3 pos)
    {
        if (IsLoaded() == false) return false;
        var inTerritory = PointInTerritoryApi(pos) ?? false;
        return inTerritory;
    }
    
    private static bool? PointInTerritoryApi(Vector3 pos)
    {
        var territory = GetCurrentTerritoryMethod.Value?.Invoke(null, [pos]);
        return territory != null;
    }
}