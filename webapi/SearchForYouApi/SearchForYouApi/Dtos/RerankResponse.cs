namespace SearchForYouApi.Dtos;

public class RerankResponse
{
    public string Id { get; set; }
    public string Model { get; set; }
    public List<RerankResult> Results { get; set; }
    // Token用量相关的可选字段
    public TokenUsage Tokens { get; set; }
    public RerankUsage Usage { get; set; }
}

public class RerankResult
{
    public Document Document { get; set; }
    public int Index { get; set; }
    public double RelevanceScore { get; set; }
}

public class Document
{
    public string Text { get; set; }
}

// Token用量相关的类（可选）
public class TokenUsage
{
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
}

public class RerankUsage
{
    public int TotalTokens { get; set; }
}