
using System;
using Newtonsoft.Json;

// json classes for discord chat logs obtained using DiscordChatExporter

public partial class Quotes
{
    [JsonProperty("guild")]
    public Guild Guild { get; set; }

    [JsonProperty("channel")]
    public Channel Channel { get; set; }

    [JsonProperty("dateRange")]
    public DateRange DateRange { get; set; }

    [JsonProperty("messages")]
    public Message[] Messages { get; set; }

    [JsonProperty("messageCount")]
    public long MessageCount { get; set; }
}

public partial class Channel
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("category")]
    public string Category { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("topic")]
    public object Topic { get; set; }
}

public partial class DateRange
{
    [JsonProperty("after")]
    public object After { get; set; }

    [JsonProperty("before")]
    public object Before { get; set; }
}

public partial class Guild
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("iconUrl")]
    public Uri IconUrl { get; set; }
}

public partial class Message
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonProperty("timestampEdited")]
    public object TimestampEdited { get; set; }

    [JsonProperty("isPinned")]
    public bool IsPinned { get; set; }

    [JsonProperty("content")]
    public string Content { get; set; }

    [JsonProperty("author")]
    public Author Author { get; set; }

    [JsonProperty("attachments")]
    public Attachment[] Attachments { get; set; }

    [JsonProperty("embeds")]
    public object[] Embeds { get; set; }

    [JsonProperty("reactions")]
    public Reaction[] Reactions { get; set; }
}

public partial class Attachment
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("fileName")]
    public string FileName { get; set; }

    [JsonProperty("fileSizeBytes")]
    public long FileSizeBytes { get; set; }
}

public partial class Author
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("discriminator")]
    public string Discriminator { get; set; }

    [JsonProperty("isBot")]
    public bool IsBot { get; set; }

    [JsonProperty("avatarUrl")]
    public Uri AvatarUrl { get; set; }
}

public partial class Reaction
{
    [JsonProperty("emoji")]
    public Emoji Emoji { get; set; }

    [JsonProperty("count")]
    public long Count { get; set; }
}

public partial class Emoji
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("isAnimated")]
    public bool IsAnimated { get; set; }

    [JsonProperty("imageUrl")]
    public Uri ImageUrl { get; set; }
}

