using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using SearchForYouApi.Dtos;

namespace SearchForYouApi.Interface;

public interface IAIService
{
     Task<string> GetAIResult(HttpResponse response,
        ChatCompletionCreateRequest chatCompletionCreate, OpenAIService openAiService);
     Task<ChatCompletionResponseUnStream> GetAIResultUnStream(ChatCompletionCreateRequest chatCompletionCreate, OpenAIService openAiService);
}