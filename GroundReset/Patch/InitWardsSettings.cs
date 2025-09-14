using GroundReset.Compatibility.WardIsLove;
using UnityEngine.SceneManagement;

namespace GroundReset.Patch;

[HarmonyPatch, HarmonyWrapSafe] 
public static class InitWardsSettings
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))] 
    private static void Init(ZNetScene __instance)
    {
        if (Helper.IsMainScene() == false) return;
        if (Helper.IsServerSafe() == false) return;

        RegisterWards();
    }

    public static void RegisterWards()
    {
        wardsSettingsList.Clear();

        AddWard("guard_stone");
        AddWardThorward();

        if (ZNetScene.instance && ZNetScene.instance.m_prefabs != null && ZNetScene.instance.m_prefabs.Count > 0)
        {
            var foundWards = ZNetScene.instance.m_prefabs.Where(x => x.GetComponent<PrivateArea>()).ToList();
            LogDebug($"Found {foundWards.Count} wards: {foundWards.GetString()}");
            foreach (var privateArea in foundWards) AddWard(privateArea.name);
        }
    }

    private static void AddWard(string name)
    {
        var prefab = ZNetScene.instance.GetPrefab(name.GetStableHashCode());
        if (!prefab) return;

        var areaComponent = prefab.GetComponent<PrivateArea>();
        if (wardsSettingsList.Exists(x => x.prefabName == name)) return;
        wardsSettingsList.Add(new WardSettings(name, areaComponent.m_radius));
    }

    private static void AddWardThorward()
    {
        var prefab = ZNetScene.instance.GetPrefab(Consts.ThorwardPrefabName.GetStableHashCode());
        if (!prefab) return;
        wardsSettingsList.Add(new WardSettings(Consts.ThorwardPrefabName, zdo =>
        {
            var radius = zdo.GetFloat(AzuWardZdoKeys.wardRadius);
            if (radius == 0) radius = WardIsLovePlugin.WardRange().Value;
            return radius;
        }));
    }
}