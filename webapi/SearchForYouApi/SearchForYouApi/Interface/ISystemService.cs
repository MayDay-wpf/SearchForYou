using Microsoft.AspNetCore.Mvc;
using OpenAI.ObjectModels.RequestModels;
using SearchForYouApi.Dtos;

namespace SearchForYouApi.Interface;

public interface ISystemService
{
    UploadImageRes UploadImage([FromForm] IFormFile file,out string msg);
    Task<string> ImgConvertToBase64(string imagePath, bool addHead = false);
    ChatCompletionCreateRequest CreateChatCompletionRequest(bool stream);
}