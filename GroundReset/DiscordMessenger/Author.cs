using YamlDotNet.Serialization;

namespace GroundReset.DiscordMessenger;

[HarmonyPatch]
[Serializable]
public class Author
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; }

    [YamlMember(Alias = "url")]
    public string Url { get; set; }

    [YamlMember(Alias = "icon_url", ApplyNamingConventions = false)]
    public string Icon { get; set; }

    [YamlMember(Alias = "proxy_icon_url", ApplyNamingConventions = false)]
    public string ProxyIcon { get; set; }
}