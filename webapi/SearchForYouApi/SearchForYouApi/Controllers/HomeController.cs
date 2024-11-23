using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SearchForYouApi.Interface;
using SearchForYouApi.Models;

namespace SearchForYouApi.Controllers;
[ApiController]
[Route("api/[controller]/[action]")]
public class HomeController : ControllerBase
{
    private readonly ILogger<HomeController> _logger;
    private readonly ISystemService _systemService;

    public HomeController(ILogger<HomeController> logger, ISystemService systemService)
    {
        _logger = logger;
        _systemService = systemService;
    }
    [HttpPost]
    public IActionResult UploadImage([FromForm] IFormFile file)
    {
        var data=_systemService.UploadImage(file,out string msg);
        return Ok(new
        {
            success = !string.IsNullOrEmpty(data.Url)?true:false,
            data=data,
            msg=msg
        });
    }
}