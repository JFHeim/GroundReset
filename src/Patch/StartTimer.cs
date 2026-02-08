namespace GroundReset.Patch;

[HarmonyPatch] 
file static class StartTimerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))] 
    private static void StartTimer()
    {
        if (Helper.IsMainScene() == false) return;
        if (Helper.IsServer(true) == false) return;

        ResetTerrainTimer.RestartTimer();
    }
}