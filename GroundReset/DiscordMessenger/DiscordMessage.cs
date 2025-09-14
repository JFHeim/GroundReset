using System.Net;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GroundReset.DiscordMessenger;

[HarmonyPatch]
[Serializable]
public class DiscordMessage
{
    [YamlMember(Alias = "username")] public string Username { get; set; }

    [YamlMember(Alias = "avatar_url", ApplyNamingConventions = false)]
    public string AvatarUrl { get; set; }

    [YamlMember(Alias = "content")] public string Content { get; set; }

    [YamlMember(Alias = "tts")] public bool TTS { get; set; }

    [YamlMember(Alias = "embeds")] public List<Embed> Embeds { get; set; } = new();

    public DiscordMessage SetUsername(string username)
    {
        Username = username;
        return this;
    }

    public DiscordMessage SetAvatar(string avatar)
    {
        AvatarUrl = avatar;
        return this;
    }

    public DiscordMessage SetContent(string content)
    {
        Content = content;
        return this;
    }

    public DiscordMessage SetTTS(bool tts)
    {
        TTS = tts;
        return this;
    }

    public Embed AddEmbed()
    {
        var embed = new Embed(this);
        Embeds.Add(embed);
        return embed;
    }

    public void SendMessage(string url)
    {
        var webClient = new WebClient();
        webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
        var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
        var yaml = serializer.Serialize(this);
        var r = new StringReader(yaml);
        var deserializer = new DeserializerBuilder().Build();
        var yamlObject = deserializer.Deserialize(r);

        var serializerJson = new SerializerBuilder().JsonCompatible().Build();
        if (yamlObject != null)
        {
            var json = serializerJson.Serialize(yamlObject);
            webClient.UploadString(url, serializerJson.Serialize(json));
        } else
        {
            throw new Exception("Failed to serialize yaml object");
        }
    }


    public void SendMessageAsync(string url)
    {
        var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
        var yaml = serializer.Serialize(this);
        var r = new StringReader(yaml);
        var deserializer = new DeserializerBuilder().Build();
        var yamlObject = deserializer.Deserialize(r);

        var serializerJson = new SerializerBuilder().JsonCompatible().Build();
        if (yamlObject != null)
        {
            var json = serializerJson.Serialize(yamlObject);
            PostToDiscord(json, url);
        } else
        {
            LogError("Failed to serialize yaml object");
        }
    }

    public static void PostToDiscord(string content, string url)
    {
        if (content == "" || url == "") return;

        var discordAPI = WebRequest.Create(url);
        discordAPI.Method = "POST";
        discordAPI.ContentType = "application/json";

        discordAPI.GetRequestStreamAsync().ContinueWith(t =>
        {
            using StreamWriter writer = new(t.Result);
            writer.WriteAsync(content).ContinueWith(_ => discordAPI.GetResponseAsync());
        });
    }
}