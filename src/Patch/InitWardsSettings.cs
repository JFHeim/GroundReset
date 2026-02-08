using GroundReset.Compatibility.WardIsLove;

namespace GroundReset.Patch;

[HarmonyPatch, HarmonyWrapSafe] 
public static class InitWardsSettings
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))] 
    private static void Init(ZNetScene __instance)
    {
        if (Helper.IsMainScene() == false) return;
        if (Helper.IsServer(true) == false) return;

        RegisterWards();
    }

    public static void RegisterWards()
    {
        Reseter.wardsSettingsList.Clear();

        AddWard("guard_stone");
        AddWardThorward();
        AddWardArcaneWard();

        if (ZNetScene.instance && ZNetScene.instance.m_prefabs != null && ZNetScene.instance.m_prefabs.Count > 0)
        {
            var foundWards = ZNetScene.instance.m_prefabs.Where(x => x.GetComponent<PrivateArea>()).ToList();
            foreach (var privateArea in foundWards) AddWard(privateArea.name);
        }
        
        LogInfo($"Found {Reseter.wardsSettingsList.Count} wards: {Reseter.wardsSettingsList.Select(x=>x.prefabName).ToArray().GetString()}");
    }

    private static void AddWard(string name)
    {
        var prefab = ZNetScene.instance.GetPrefab(name.GetStableHashCode());
        if (!prefab) return;

        var areaComponent = prefab.GetComponent<PrivateArea>();
        if (Reseter.wardsSettingsList.Any(x => x.prefabName == name)) return;
        Reseter.wardsSettingsList.Add(new WardSettings(name, areaComponent.m_radius));
    }

    private static void AddWardThorward()
    {
        var prefab = ZNetScene.instance.GetPrefab(Consts.ThorwardPrefabName.GetStableHashCode());
        if (!prefab) return;
        Reseter.wardsSettingsList.Add(new WardSettings(Consts.ThorwardPrefabName, zdo =>
        {
            var radius = zdo.GetFloat(AzuWardZdoKeys.wardRadius);
            if (radius == 0) radius = WardIsLovePlugin.WardRange().Value;
            return radius;
        }));
    }

    private static void AddWardArcaneWard()
    {
        var prefab = ZNetScene.instance.GetPrefab(Consts.ArcaneWardPrefabName.GetStableHashCode());
        if (!prefab) return;
        Reseter.wardsSettingsList.Add(new WardSettings(Consts.ArcaneWardPrefabName, zdo =>
        {
            var radius = zdo.GetInt(Consts.ArcaneWardZdoKey);

            // I don't feel motivated enough to dig into thoughts behind why radius being zero should be overwritten, but let's keep an eye on it
            // https://github.com/shudnal/GroundReset/commit/7e0e3970400e5f617c796862f311982860ce6ccc#diff-e85d0c645d39150699ea1c29728847fafcea54e5277789ca401294d42a0e4de7R69
            // if (radius == 0) return 64;

            return radius;
        }));
    }
}