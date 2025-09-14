using System.Diagnostics;
using System.Threading.Tasks;
using GroundReset.Compatibility.kgMarketplace;

namespace GroundReset;

public static class Reseter
{
    public const string terrCompPrefabName = "_TerrainCompiler";
    private static readonly int RadiusNetKey = "wardRadius".GetStableHashCode();

    internal static readonly int HeightmapWidth = 64;
    internal static readonly int HeightmapScale = 1;
    private static List<ZDO> wards = new();
    public static List<WardSettings> wardsSettingsList = new();
    public static Stopwatch watch = new();

    internal static async void ResetAll(bool checkIfNeed = true,
        bool checkWards = true, bool ranFromConsole = false)
    {
        await FindWards();
        await Terrains.ResetTerrains(checkWards);

        if (ranFromConsole) Console.instance.AddString("<color=green> Done </color>");
        wards.Clear();
    }

    private static async Task FindWards()
    {
        watch.Restart();
        wards.Clear();
        for (var i = 0; i < wardsSettingsList.Count; i++)
        {
            var wardsSettings = wardsSettingsList[i];
            var zdos = await ZoneSystem.instance.GetWorldObjectsAsync(wardsSettings.prefabName);
            wards = wards.Concat(zdos).ToList();
        }

        var totalSeconds = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
        DebugWarning($"Wards count: {wards.Count}. Took {totalSeconds} seconds");
        watch.Restart();
    }

    public static Vector3 HmapToWorld(Vector3 heightmapPos, int x, int y)
    {
        var xPos = ((float)x - HeightmapWidth / 2) * HeightmapScale;
        var zPos = ((float)y - HeightmapWidth / 2) * HeightmapScale;
        return heightmapPos + new Vector3(xPos, 0f, zPos);
    }

    public static void WorldToVertex(Vector3 worldPos, Vector3 heightmapPos, out int x, out int y)
    {
        var vector3 = worldPos - heightmapPos;
        x = FloorToInt((float)(vector3.x / (double)HeightmapScale + 0.5)) + HeightmapWidth / 2;
        y = FloorToInt((float)(vector3.z / (double)HeightmapScale + 0.5)) + HeightmapWidth / 2;
    }

    public static IEnumerator SaveTime()
    {
        yield return new WaitForSeconds(savedTimeUpdateInterval);

        timePassedInMinutesConfig.Value = timer.Timer / 60;
        ZNetScene.instance.StartCoroutine(SaveTime());
    }

    public static bool IsInWard(Vector3 pos, float checkRadius = 0)
    {
        return wards.Exists(searchWard =>
        {
            var wardSettings =
                wardsSettingsList.Find(s => s.prefabName.GetStableHashCode() == searchWard.GetPrefab());
            var isEnabled = searchWard.GetBool(ZDOVars.s_enabled);
            if (!isEnabled) return false; // not enabled, skip range check
            var wardRadius = wardSettings.dynamicRadius
                ? wardSettings.getDynamicRadius(searchWard)
                : wardSettings.radius;
            var inRange = pos.DistanceXZ(searchWard.GetPosition()) <= wardRadius + checkRadius;
            return inRange;
        }) || MarketplaceTerritorySystem.PointInTerritory(pos);
    }

    public static bool IsInWard(Vector3 zoneCenter, int w, int h) { return IsInWard(HmapToWorld(zoneCenter, w, h)); }
}