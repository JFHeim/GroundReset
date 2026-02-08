namespace GroundReset.Patch;

[HarmonyPatch] 
file static class SaveTimerTimePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Save))] 
    private static void SaveTime()
    {
        if (Helper.IsServer(true) == false) return;
        
        ResetTerrainTimer.SavePassedTimerTimeToFile();
    }
}