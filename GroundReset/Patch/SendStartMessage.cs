using GroundReset.DiscordMessenger;

namespace GroundReset.Patch;

[HarmonyPatch]
public static class SendStartMessage
{
    [HarmonyPatch(typeof(Game), nameof(Game.Start))] [HarmonyWrapSafe] [HarmonyPostfix]
    private static void Postfix(Game __instance) => Discord.SendStartMessage();
}