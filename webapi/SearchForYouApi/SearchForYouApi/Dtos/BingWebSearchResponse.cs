using System.Text.Json.Serialization;

namespace SearchForYouApi.Dtos;

public class BingWebSearchResponse
{
    [JsonPropertyName("_type")]
    public string Type { get; set; }
    public QueryContext QueryContext { get; set; }
    public WebPages WebPages { get; set; }
    public List<ImageResult> value { get; set; }
    public Videos Videos { get; set; }
    public RankingResponse RankingResponse { get; set; }
}

public class QueryContext
{
    public string OriginalQuery { get; set; }
}

public class WebPages
{
    public string WebSearchUrl { get; set; }
    public int TotalEstimatedMatches { get; set; }
    public List<WebPage> Value { get; set; }
    public bool SomeResultsRemoved { get; set; }
}

public class WebPage
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public string ThumbnailUrl { get; set; }
    public string DatePublished { get; set; }
    public string DatePublishedDisplayText { get; set; }
    public bool IsFamilyFriendly { get; set; }
    public string DisplayUrl { get; set; }
    public string Snippet { get; set; }
    public string DateLastCrawled { get; set; }
    public PrimaryImageOfPage PrimaryImageOfPage { get; set; }
    public List<DeepLink> DeepLinks { get; set; }
    public List<Breadcrumb> Breadcrumb { get; set; }
    public string Language { get; set; }
    public bool IsNavigational { get; set; }
    public string CachedPageUrl { get; set; }
    public bool NoCache { get; set; }
    public string SiteName { get; set; }
}

public class PrimaryImageOfPage
{
    public string ThumbnailUrl { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int SourceWidth { get; set; }
    public int SourceHeight { get; set; }
    public string ImageId { get; set; }
}

public class DeepLink
{
    public string Name { get; set; }
    public string Url { get; set; }
    public string Snippet { get; set; }
}

public class Breadcrumb
{
    public string Text { get; set; }
    public string Url { get; set; }
}

public class Images
{
    public string Id { get; set; }
    public string ReadLink { get; set; }
    public string WebSearchUrl { get; set; }
    public bool IsFamilyFriendly { get; set; }
    public List<ImageResult> Value { get; set; }
    public string name { get; set; }
}

public class ImageResult
{
    public string WebSearchUrl { get; set; }
    public string Name { get; set; }
    public string ThumbnailUrl { get; set; }
    public string DatePublished { get; set; }
    public string ContentUrl { get; set; }
    public string HostPageUrl { get; set; }
    public string ContentSize { get; set; }
    public string EncodingFormat { get; set; }
    public string HostPageDisplayUrl { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public Thumbnail Thumbnail { get; set; }
}

public class Thumbnail
{
    public int Width { get; set; }
    public int Height { get; set; }
}

public class Videos
{
    public string Id { get; set; }
    public string ReadLink { get; set; }
    public string WebSearchUrl { get; set; }
    public bool IsFamilyFriendly { get; set; }
    public List<VideoResult> Value { get; set; }
    public string Scenario { get; set; }
}

public class VideoResult
{
    public string WebSearchUrl { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ThumbnailUrl { get; set; }
    public string DatePublished { get; set; }
    public List<Publisher> Publisher { get; set; }
    public Creator Creator { get; set; }
    public string ContentUrl { get; set; }
    public string HostPageUrl { get; set; }
    public string EncodingFormat { get; set; }
    public string HostPageDisplayUrl { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Duration { get; set; }
    public string EmbedHtml { get; set; }
    public bool AllowHttpsEmbed { get; set; }
    public int ViewCount { get; set; }
    public Thumbnail Thumbnail { get; set; }
    public bool AllowMobileEmbed { get; set; }
    public bool IsSuperfresh { get; set; }
}

public class Publisher
{
    public string Name { get; set; }
}

public class Creator
{
    public string Name { get; set; }
}

public class RankingResponse
{
    public Mainline Mainline { get; set; }
}

public class Mainline
{
    public List<Item> Items { get; set; }
}

public class Item
{
    public string AnswerType { get; set; }
    public int? ResultIndex { get; set; }
    public Value Value { get; set; }
}

public class Value
{
    public string Id { get; set; }
}
