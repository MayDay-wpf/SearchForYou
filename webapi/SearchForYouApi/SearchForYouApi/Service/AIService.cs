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

}