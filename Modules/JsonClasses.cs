using System;
using Newtonsoft.Json;

public partial class Quotes
{
    [JsonProperty("messages")]
    public Message[] Messages { get; set; }

    [JsonProperty("messageCount")]
    public long MessageCount { get; set; }
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
    public object[] Attachments { get; set; }

    [JsonProperty("embeds")]
    public object[] Embeds { get; set; }

    [JsonProperty("reactions")]
    public Reaction[] Reactions { get; set; }
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
    public object Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("isAnimated")]
    public bool IsAnimated { get; set; }

    [JsonProperty("imageUrl")]
    public Uri ImageUrl { get; set; }
}


/*
namespace Botkic.Modules
{
    public class User
        {
            [JsonProperty("id")]
            public string id { get; }
            [JsonProperty("name")]
            public string name { get; }
            [JsonProperty("discriminator")]
            public string discriminator { get; }
            [JsonProperty("isBot")]
            public bool isBot { get; }
            [JsonProperty("avatarUrl")]
            public string avatarUrl { get; } 
        }

    public class Reaction
    {
        public class Emote {
            [JsonProperty("id")]
            public object id { get; }
            [JsonProperty("name")]
            public string name { get; }
            [JsonProperty("isAnimated")]
            public bool isAnimated { get; }
            [JsonProperty("imageUrl")]
            public string imageUrl { get; }
        }
        [JsonProperty("emoji")]
        public Emote emoji { get; }
        [JsonProperty("count")]
        public int count { get; }
    }

    public class Message
    {
        [JsonProperty("id")]
        public string id { get; }
        [JsonProperty("type")]
        public string type { get; }
        [JsonProperty("timestamp")]
        public DateTime timestamp { get; }
        [JsonProperty("timestampEdited")]
        public object timestampEdited { get; }
        [JsonProperty("bool")]
        public bool isPinned { get; }
        [JsonProperty("content")]
        public string content { get; }
        [JsonProperty("author")]
        public User author { get; }
        [JsonProperty("attachments")]
        public IList<string> attachments { get; }
        [JsonProperty("embeds")]
        public IList<string> embeds { get; }
        [JsonProperty("reactions")]
        public IList<Reaction> reactions { get; }
    }

    public class Quotes
    {
        [JsonProperty("messages")]
        public IList<Message> messages { get; }
        [JsonProperty("messageCount")]
        public int messageCount { get; }
    }

    public class Wrapper
    {
        [JsonProperty("JsonValues")]
        public Quotes Quotes { get; }
    }
} */
