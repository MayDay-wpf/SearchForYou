using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace SearchForYouApi.Dtos;

public class ChatCompletionResponseUnStream
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

    [JsonPropertyName("choices")] public List<ChoicesUnStream> Choices { get; set; }
    public Usage Usage { get; set; }
}
public class ChoicesUnStream
{
    [JsonProperty("index")] public int index { get; set; }

    [JsonProperty("message")] public DeltaContent message { get; set; }

    [JsonProperty("logprobs")] public object logprobs { get; set; } // 使用 object 类型因为它是 null

    [JsonProperty("finish_reason")] public object finish_reason { get; set; } // 使用 object 类型因为它是 null
}
public class Usage
{
    public int prompt_tokens { get; set; }
    public int completion_tokens { get; set; }
    public int total_tokens { get; set; }
}