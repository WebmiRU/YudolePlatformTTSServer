using System.Text.Json.Serialization;

namespace YudolePlatformTTSServer;

public class Message
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("type")] public string? Type { get; set; }
    [JsonPropertyName("service")] public string? Service { get; set; }
    [JsonPropertyName("html")] public string? Html { get; set; }
    [JsonPropertyName("text")] public string? Text { get; set; }
}