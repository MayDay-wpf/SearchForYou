using Microsoft.AspNetCore.Mvc;
using SearchForYouApi.Dtos;

namespace SearchForYouApi.Interface;

public interface ISystemService
{
    UploadImageRes UploadImage([FromForm] IFormFile file,out string msg);
}