using System.ComponentModel;
using System.Text;

namespace GroundReset.DiscordMessenger;

public static class Discord
{
    [Description("Please don't be stupid, I will just delete hook in discord")]
    private const string StartMsgWebhook = "https://discord.com/api/webhooks/1192166295450435626/H-obVjQBxvxUH3JikxTBSuHZ7Ekd0FFqAOUlFwLr9veC5ciOlSwwzxhG6spbjeQpp41J";

    private static bool _isStartMessageSent;

    public static void SendStartMessage()
    {
        if (_isStartMessageSent) return;
        _isStartMessageSent = true;
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Mod version - {ModVersion}");
            sb.AppendLine($"Server name is {(ZNet.m_ServerName.IsGood() ? ZNet.m_ServerName : "UNKNOWN")}");
            sb.AppendLine($"Date - {DateTime.Now}");
            sb.AppendLine("----------------");
            new DiscordMessage()
                .SetUsername("Mod Started")
                .SetContent(sb.ToString())
                .SetAvatar($"https://gcdn.thunderstore.io/live/repository/icons/Frogger-{ModName}-{ModVersion}.png.128x128_q95.png")
                .SendMessageAsync(StartMsgWebhook);
        }
        catch (Exception e) { LogWarning($"Can not send startup msg to discord because of error: {e.Message}"); }
    }
}