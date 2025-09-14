using YamlDotNet.Serialization;

namespace GroundReset.DiscordMessenger;

[HarmonyPatch]
[Serializable]
public class Embed
{
    public Embed() { }

    public Embed(DiscordMessage messageInstance) { MessageInstance = messageInstance; }

    [YamlMember(Alias = "title")] public string Title { get; set; } = null!;

    [YamlMember(Alias = "description")] public string Description { get; set; } = null!;

    [YamlMember(Alias = "url")] public string Url { get; set; } = null!;

    [YamlMember(Alias = "timestamp")] public string Timestamp { get; set; } = null!;

    [YamlMember(Alias = "color")] public int Color { get; set; }

    [YamlMember(Alias = "footer")] public Footer Footer { get; set; } = null!;

    [YamlMember(Alias = "image")] public Image Image { get; set; } = null!;

    [YamlMember(Alias = "thumbnail")] public Thumbnail Thumbnail { get; set; } = null!;

    [YamlMember(Alias = "video")] public Video Video { get; set; } = null!;

    [YamlMember(Alias = "provider")] public Provider Provider { get; set; } = null!;

    [YamlMember(Alias = "author")] public Author Author { get; set; } = null!;

    [YamlMember(Alias = "fields")] public List<Field> Fields { get; set; } = new();

    [YamlIgnore] private DiscordMessage MessageInstance { get; set; } = null!;

    public Embed SetTitle(string title)
    {
        Title = title;
        return this;
    }

    public Embed SetDescription(string description)
    {
        Description = description;
        return this;
    }

    public Embed SetUrl(string url)
    {
        Url = url;
        return this;
    }

    public Embed SetTimestamp(DateTime time)
    {
        Timestamp = time.ToString("yyyy-MM-ddTHH:mm:ssZ");
        return this;
    }

    public Embed SetColor(int color)
    {
        Color = color;
        return this;
    }

    public Embed SetFooter(string text, string icon = null!, string proxyIcon = null!)
    {
        Footer = new Footer { Text = text, Icon = icon, ProxyIcon = proxyIcon };
        return this;
    }

    public Embed SetImage(string url, string height = null!, string width = null!, string proxyIcon = null!)
    {
        Image = new Image { Url = url, Height = height, Width = width, ProxyIcon = proxyIcon };
        return this;
    }

    public Embed SetThumbnail(string url, string height = null!, string width = null!, string proxyIcon = null!)
    {
        Thumbnail = new Thumbnail { Url = url, Height = height, Width = width, ProxyIcon = proxyIcon };
        return this;
    }

    public Embed SetVideo(string url, string height = null!, string width = null!, string proxyVideo = null!)
    {
        Video = new Video { Url = url, Height = height, Width = width, ProxyVideo = proxyVideo };
        return this;
    }

    public Embed SetProvider(string name, string url = null!)
    {
        Provider = new Provider { Name = name, Url = url };
        return this;
    }

    public Embed SetAuthor(string name, string url = null!, string icon = null!, string proxyIcon = null!)
    {
        Author = new Author { Name = name, Icon = icon, ProxyIcon = proxyIcon, Url = url };
        return this;
    }

    public Embed AddField(string key, string value, bool inline = false)
    {
        Fields.Add(new Field
        {
            Key = key,
            Value = value,
            Inline = inline
        });
        return this;
    }

    public DiscordMessage Build() { return MessageInstance; }
}