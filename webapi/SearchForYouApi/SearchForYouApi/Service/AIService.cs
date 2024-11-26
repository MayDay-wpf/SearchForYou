using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels.ResponseModels;
using SearchForYouApi.Dtos;
using SearchForYouApi.Interface;

namespace SearchForYouApi.Service;

public class AIService: IAIService
{
    private readonly IConfiguration _configuration;
    public AIService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public async Task<string> GetAIResult(HttpResponse response,
        ChatCompletionCreateRequest chatCompletionCreate, OpenAIService openAiService)
    {
            var completionResult = openAiService.ChatCompletion.CreateCompletionAsStream(chatCompletionCreate);
            var intent = string.Empty;
            await foreach (var responseContent in completionResult)
            {
                if (responseContent.Successful)
                {
                    var choice = responseContent.Choices.FirstOrDefault();
                    if (choice != null)
                    {
                        var chatCompletionResponse = CreateOpenAIStreamResult(responseContent);
                        if (chatCompletionResponse.Choices.FirstOrDefault() != null &&
                            !string.IsNullOrEmpty(chatCompletionResponse.Choices
                                .FirstOrDefault().delta.Content))
                        {
                            intent += chatCompletionResponse.Choices
                                .FirstOrDefault().delta.Content;
                            chatCompletionResponse.Model = chatCompletionCreate.Model;
                            var msgBytes = CreateStream(chatCompletionResponse);
                            await SendStream(response, msgBytes);
                        }
                    }
                }
            }
            return intent;
    }
    public async Task<ChatCompletionResponseUnStream> GetAIResultUnStream(ChatCompletionCreateRequest chatCompletionCreate, OpenAIService openAiService)
    {
        var chatCompletionResponse = new ChatCompletionResponseUnStream();
        var completionResult = await openAiService.ChatCompletion.CreateCompletion(chatCompletionCreate);
        if (completionResult.Successful)
        {
            completionResult.Model = chatCompletionCreate.Model;
            chatCompletionResponse = CreateOpenAIUnStreamResult(completionResult);
        }
        return chatCompletionResponse;
    }
    public ChatCompletionResponseUnStream CreateOpenAIUnStreamResult(ChatCompletionCreateResponse responseContent)
    {
        var chatCompletionResponse = new ChatCompletionResponseUnStream();
        chatCompletionResponse.Id = responseContent.Id;
        chatCompletionResponse.Object = responseContent.ObjectTypeName;
        chatCompletionResponse.Created = responseContent.CreatedAt;
        chatCompletionResponse.Model = responseContent.Model;
        chatCompletionResponse.system_fingerprint = responseContent.SystemFingerPrint;
        var chatChoices = new List<ChoicesUnStream>();
        foreach (var item in responseContent.Choices)
        {
            var chatChoiceResponse = new ChoicesUnStream();
            chatChoiceResponse.index = item.Index.Value;
            chatChoiceResponse.finish_reason = item.FinishReason;
            var delta = new DeltaContent();
            if (item.Message != null)
            {
                delta.Content = item.Message.Content;
                delta.Role = item.Message.Role;
                chatChoiceResponse.message = delta;
            }

            chatChoices.Add(chatChoiceResponse);
        }

        chatCompletionResponse.Choices = chatChoices;
        chatCompletionResponse.Usage = new Dtos.Usage
        {
            completion_tokens = (int)responseContent.Usage.CompletionTokens,
            prompt_tokens = responseContent.Usage.PromptTokens,
            total_tokens = responseContent.Usage.TotalTokens
        };
        return chatCompletionResponse;
    }
    private ChatCompletionResponse CreateOpenAIStreamResult(ChatCompletionCreateResponse responseContent)
    {
        var chatCompletionResponse = new ChatCompletionResponse();
        chatCompletionResponse.Id = responseContent.Id;
        chatCompletionResponse.Object = responseContent.ObjectTypeName;
        chatCompletionResponse.Created = responseContent.CreatedAt;
        chatCompletionResponse.Model = responseContent.Model;
        chatCompletionResponse.system_fingerprint = responseContent.SystemFingerPrint;
        var chatChoices = new List<Choices>();
        foreach (var item in responseContent.Choices)
        {
            var chatChoiceResponse = new Choices();
            chatChoiceResponse.index = item.Index.Value;
            chatChoiceResponse.finish_reason = item.FinishReason;
            var delta = new DeltaContent();
            if (item.Delta != null)
            {
                delta.Content = item.Delta.Content;
                delta.Role = item.Delta.Role;
                chatChoiceResponse.delta = delta;
            }

            chatChoices.Add(chatChoiceResponse);
        }

        chatCompletionResponse.Choices = chatChoices;
        return chatCompletionResponse;
    }
    private byte[] CreateStream(ChatCompletionResponse chatCompletionResponse)
    {
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };
        var jsonContent = JsonConvert.SerializeObject(chatCompletionResponse, settings);
        var msg = $"data: {jsonContent}\n\n";
        var msgBytes = Encoding.UTF8.GetBytes(msg);
        return msgBytes;
    }
    private async Task SendStream(HttpResponse response, byte[] msgBytes)
    {
        await response.Body.WriteAsync(msgBytes,
            0,
            msgBytes.Length);
        await response.Body.FlushAsync(); // 确保立即发送消息
    }
    public async Task<RerankResponse> GetRerankResponse(string query, List<string> doc, int topn = 1)
    {
        var rerankResponse = new RerankResponse();
        var apikey = _configuration.GetValue<string>("Reranker:ApiKey");
        var baseUrl = _configuration.GetValue<string>("Reranker:BaseUrl");
        var model= _configuration.GetValue<string>("Reranker:Model");
        try
        {
            // 构建请求体
            var requestBody = new
            {
                model = model,
                query = query,
                top_n = topn,
                documents = doc,
                return_documents = true
            };

            // 序列化请求体
            var jsonBody = JsonConvert.SerializeObject(requestBody);

            using (var client = new HttpClient())
            {
                // 只添加 Authorization 头
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apikey}");
    
                // Content-Type 已经在 StringContent 构造函数中设置
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
    
                var response = await client.PostAsync(baseUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();
    
                if (response.IsSuccessStatusCode)
                {
                    rerankResponse = JsonConvert.DeserializeObject<RerankResponse>(responseContent);
                }
                else
                {
                    throw new Exception($"Rerank API request failed with status code: {response.StatusCode}, Message: {responseContent}");
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error in GetRerankResponse: {ex.Message}", ex);
        }

        return rerankResponse;
    }
    public async Task<string> JinaUrlRead(string url)
    {
        var apikey = _configuration.GetValue<string>("JinaUrlRead:ApiKey");
        var baseUrl = _configuration.GetValue<string>("JinaUrlRead:BaseUrl");

        // 构造完整的请求URL
        string fullUrl = $"{baseUrl}/{url}";

        try
        {
            using (var client = new HttpClient())
            {
                // 正确设置 Bearer token 认证头
                client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", apikey);

                // 发送GET请求
                var response = await client.GetAsync(fullUrl);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
                }
            }
        }
        catch (Exception ex)
        {
            // 处理异常
            throw new Exception($"Error reading URL: {ex.Message}", ex);
        }
    }

}