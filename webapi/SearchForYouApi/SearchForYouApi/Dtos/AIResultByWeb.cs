namespace SearchForYouApi.Dtos;

public class AIResultByWeb
{
    public List<SearchEngineResult> SearchEngineResultList { get; set; }
    public string? ImageUrl { get; set; }
    public string? Question { get; set; }
    public string? Intant { get; set; }
}