using YamlDotNet.Serialization;

namespace GroundReset.DiscordMessenger;

[HarmonyPatch]
[Serializable]
public class Footer
{
    [YamlMember(Alias = "text")] public string Text { get; set; }

    [YamlMember(Alias = "icon_url", ApplyNamingConventions = false)]
    public string Icon { get; set; }

    [YamlMember(Alias = "proxy_icon_url", ApplyNamingConventions = false)]
    public string ProxyIcon { get; set; }
}