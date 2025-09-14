using GroundReset.Compatibility.WardIsLove;
using UnityEngine.SceneManagement;

namespace GroundReset.Patch;

[HarmonyPatch] internal class InitWardsSettings
{
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))] [HarmonyPostfix] [HarmonyWrapSafe]
    internal static void Init(ZNetScene __instance)
    {
        if (SceneManager.GetActiveScene().name != "main") return;
        if (!ZNet.instance.IsServer()) return;

        RegisterWards();
    }

    internal static void RegisterWards()
    {
        wardsSettingsList.Clear();

        AddWard("guard_stone");
        AddWardThorward();

        if (ZNetScene.instance && ZNetScene.instance.m_prefabs != null && ZNetScene.instance.m_prefabs.Count > 0)
        {
            var foundWards = ZNetScene.instance.m_prefabs.Where(x => x.GetComponent<PrivateArea>()).ToList();
            Debug($"Found {foundWards.Count} wards: {foundWards.GetString()}");
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
        var name = "Thorward";
        var prefab = ZNetScene.instance.GetPrefab(name.GetStableHashCode());
        if (!prefab) return;
        wardsSettingsList.Add(new WardSettings(name, zdo =>
        {
            var radius = zdo.GetFloat(AzuWardZdoKeys.wardRadius);
            if (radius == 0) return WardIsLovePlugin.WardRange().Value;
            return radius;
        }));
    }
}