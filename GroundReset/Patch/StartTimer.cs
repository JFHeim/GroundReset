namespace GroundReset.Patch;

[HarmonyPatch] 
file static class StartTimerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))] 
    private static void StartTimer()
    {
        if (Helper.IsMainScene() == false) return;
        if (Helper.IsServerSafe() == false) return;

        ResetTerrainTimer.RestartTimer();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Save))] 
    private static void SaveTime()
    {
        if (Helper.IsServerSafe() == false) return;
        
        ResetTerrainTimer.SavePassedTimerTimeToFile();
    }
}