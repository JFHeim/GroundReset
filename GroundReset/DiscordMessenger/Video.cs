using YamlDotNet.Serialization;

namespace GroundReset.DiscordMessenger;

[HarmonyPatch]
[Serializable]
public class Video
{
    [YamlMember(Alias = "url")] public string Url { get; set; }

    [YamlMember(Alias = "proxy_url", ApplyNamingConventions = false)]
    public string ProxyVideo { get; set; }

    [YamlMember(Alias = "width")] public string Width { get; set; }

    [YamlMember(Alias = "height")] public string Height { get; set; }
}