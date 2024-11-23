using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace SearchForYouApi.Dtos;

public class ChatCompletionResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("object")]
    public string Object { get; set; }

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("system_fingerprint")]
    public string system_fingerprint { get; set; }

    [JsonPropertyName("choices")]
    public List<Choices> Choices { get; set; }
}
public class Choices
{
    [JsonProperty("index")]
    public int index { get; set; }

    [JsonProperty("delta")]
    public DeltaContent delta { get; set; }

    [JsonProperty("logprobs")]
    public object logprobs { get; set; } 

    [JsonProperty("finish_reason")]
    public object finish_reason { get; set; }
}
public class DeltaContent
{
    [JsonProperty("content")] public string Content { get; set; }
    [JsonProperty("role")] public string Role { get; set; }
}
