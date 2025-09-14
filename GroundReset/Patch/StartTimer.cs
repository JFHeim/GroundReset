using UnityEngine.SceneManagement;

namespace GroundReset.Patch;

[HarmonyPatch] internal class StartTimer
{
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))] [HarmonyPostfix]
    private static void ZNetSceneAwake_StartTimer(ZNetScene __instance)
    {
        if (SceneManager.GetActiveScene().name != "main") return;
        if (!ZNet.instance.IsServer()) return;

        float time;
        if (Plugin.timePassedInMinutes > 0) time = Plugin.timePassedInMinutes;
        else time = Plugin.timeInMinutes;
        time *= 60;

        FunctionTimer.Create(Plugin.onTimer, time, "JF_GroundReset", true, true);
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Shutdown))] [HarmonyPostfix]
    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Awake))]
    private static void ZNet_SaveTime(ZNet __instance)
    {
        if (!ZNet.instance.IsServer()) return;
        __instance.StartCoroutine(SaveTime());
    }
}