namespace SearchForYouApi.Middleware;

public class IPCheck
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    public IPCheck(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task Invoke(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress.ToString();
        var ipList = _configuration.GetSection("WhiteIPList").Get<List<string>>();
        if (ipList.Contains(ip) || ipList.Contains("*"))
        {
            await _next(context);
        }
        else
        {
            context.Response.StatusCode = 403;
        }
    }
}