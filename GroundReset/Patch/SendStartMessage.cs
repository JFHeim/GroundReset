using GroundReset.DiscordMessenger;

namespace GroundReset.Patch;

[HarmonyPatch, HarmonyWrapSafe]
public static class SendStartMessage
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Game), nameof(Game.Start))] 
    private static void Postfix(Game __instance) => Discord.SendStartMessage();
}