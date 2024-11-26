using System.Text;
using System.Text.Json;
using Google.Apis.CustomSearchAPI.v1;
using Google.Apis.Services;
using Microsoft.Azure.CognitiveServices.Search.ImageSearch;
using Microsoft.Azure.CognitiveServices.Search.WebSearch;
using Microsoft.OpenApi.Services;
using SearchForYouApi.Dtos;
using SearchForYouApi.Interface;
using ApiKeyServiceClientCredentials = Microsoft.Azure.CognitiveServices.Search.ImageSearch.ApiKeyServiceClientCredentials;

namespace SearchForYouApi.Service;

public class SearchEngineService: ISearchEngineService
{
    private readonly IConfiguration _configuration;
    private readonly IAIService _aiService;
    public SearchEngineService(IConfiguration configuration, IAIService aiService)
    {
        _configuration = configuration;
        _aiService = aiService;
    }
    public async Task<List<SearchEngineResult>> GoogleSearch(string query,string imageUrl,bool onlyImg = false)
    {
        var apiKey= _configuration.GetValue<string>("GoogleEngine:ApiKey");
        var searchEngineId= _configuration.GetValue<string>("GoogleEngine:EngineId");
        // 创建一个CustomsearchService实例
        var customsearchService = new CustomSearchAPIService(new BaseClientService.Initializer
        {
            ApiKey = apiKey
        });
                           
        // 构建搜索请求
        var listRequest = customsearchService.Cse.List();
        listRequest.Cx = searchEngineId;
        listRequest.Q = query;
        if (onlyImg)
        {
            listRequest.SearchType = CseResource.ListRequest.SearchTypeEnum.Image;
            listRequest.Q= imageUrl;
        }
                           
        // 发送搜索请求并获取结果
        var searchResult = listRequest.Execute();
                           
        // 处理搜索结果
        var results = new List<SearchEngineResult>();
        foreach (var item in searchResult.Items)
        {
            results.Add(new SearchEngineResult
            {
                Title = item.Title,
                Url = item.Link,
                Snippet = item.Snippet
            });
        }
                           
        return results;
    }

    public async Task<List<SearchEngineResult>> BingSearch(string query, string imageUrl, bool onlyImg = false)
    {
        var apiKey = _configuration.GetValue<string>("BingEngine:ApiKey");
        var searchResults = new List<SearchEngineResult>();
        var baseUrl = _configuration.GetValue<string>("BingEngine:EndPoint");
        
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);

            try
            {
                HttpResponseMessage response;
                if (onlyImg)
                {
                    // 图片搜索
                    var imageSearchUrl = $"{baseUrl}/images/search?q={Uri.EscapeDataString(imageUrl)}&count=10";
                    response = await client.GetAsync(imageSearchUrl);
                }
                else
                {
                    // 网页搜索
                    var webSearchUrl = $"{baseUrl}/search?q={Uri.EscapeDataString(query)}&count=10";
                    response = await client.GetAsync(webSearchUrl);
                }

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                if (onlyImg)
                {
                    var imageResults = JsonSerializer.Deserialize<BingWebSearchResponse>(content, options);
                    if (imageResults?.value != null)
                    {
                        foreach (var result in imageResults.value)
                        {
                            searchResults.Add(new SearchEngineResult
                            {
                                Title = result.Name,
                                Url = result.ContentUrl
                            });
                        }
                    }
                }
                else
                {
                    var webResults = JsonSerializer.Deserialize<BingWebSearchResponse>(content, options);
                    if (webResults?.WebPages?.Value != null)
                    {
                        foreach (var result in webResults.WebPages.Value)
                        {
                            searchResults.Add(new SearchEngineResult
                            {
                                Title = result.Name,
                                Url = result.Url,
                                Snippet = result.Snippet,
                            });
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                throw;
            }
            catch (JsonException ex)
            {
                throw;
            }
        }

        return searchResults;
    } 
    public async Task<List<SearchEngineResult>> SerperSearch(string query, string imageUrl, bool onlyImg = false)
        {
            var apiKey = _configuration.GetValue<string>("SerperEngine:ApiKey");
            var client = new HttpClient();
            var results = new List<SearchEngineResult>();

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, 
                    onlyImg ? "https://google.serper.dev/images" : "https://google.serper.dev/search");
                request.Headers.Add("X-API-KEY", apiKey);

                var content = new StringContent(
                    JsonSerializer.Serialize(new { q = onlyImg ? imageUrl : query, hl = "zh-cn" }), 
                    Encoding.UTF8, 
                    "application/json");
                request.Content = content;

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                
                var jsonResponse = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                
                if (onlyImg)
                {
                    // 处理图片搜索结果
                    var images = jsonResponse.RootElement.GetProperty("images");
                    foreach (var image in images.EnumerateArray())
                    {
                        results.Add(new SearchEngineResult
                        {
                            Title = image.GetProperty("title").GetString(),
                            Url = image.GetProperty("imageUrl").GetString(), // 图片搜索返回imageUrl
                        });
                    }
                }
                else
                {
                    // 处理普通搜索结果
                    var organic = jsonResponse.RootElement.GetProperty("organic");
                    foreach (var item in organic.EnumerateArray())
                    {
                        results.Add(new SearchEngineResult
                        {
                            Title = item.GetProperty("title").GetString(),
                            Url = item.GetProperty("link").GetString(),
                            Snippet = item.GetProperty("snippet").GetString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                
            }

            return results;
        }

    public async Task<List<SearchEngineResult>> RerankWithDuplicates(string query, List<SearchEngineResult> searchResults)
    {
        // 参数验证
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("查询不能为空。", nameof(query));
        }

        if (searchResults == null || !searchResults.Any())
        {
            return new List<SearchEngineResult>();
        }

        try
        {
            // 1. 提取所有 Snippet 用于重排序
            var documents = searchResults.Select(r => r.Snippet ?? string.Empty).ToList();

            // 2. 调用重排序API
            var rerankResponse = await _aiService.GetRerankResponse(query, documents, topn: searchResults.Count);

            if (rerankResponse?.Results == null || !rerankResponse.Results.Any())
            {
                // 如果重排序失败，返回原始结果
                return searchResults;
            }

            // 3. 创建一个包含结果和分数的临时类
            var rerankedResults = rerankResponse.Results
                .Where(rerankResult => rerankResult.Index >= 0 && rerankResult.Index < searchResults.Count)
                .Select(rerankResult => new RankedResult
                {
                    Result = searchResults[rerankResult.Index],
                    Score = rerankResult.RelevanceScore
                })
                .OrderByDescending(x => x.Score)
                .Select(x => x.Result)
                .ToList();

            return rerankedResults;
        }
        catch (Exception ex)
        {
            return searchResults;
        }
    }
    private class RankedResult
    {
        public SearchEngineResult Result { get; set; }
        public double Score { get; set; }
    }
}