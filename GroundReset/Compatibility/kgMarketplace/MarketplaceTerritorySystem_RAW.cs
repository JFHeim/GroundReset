using Marketplace.Modules.TerritorySystem;

namespace GroundReset.Compatibility.kgMarketplace;

// ReSharper disable once InconsistentNaming
public static class MarketplaceTerritorySystem_RAW
{
    public static bool PointInTerritory(Vector3 pos) => TerritorySystem_DataTypes.Territory.GetCurrentTerritory(pos) != null;
}