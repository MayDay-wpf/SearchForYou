using SearchForYouApi.Interface;
using SearchForYouApi.Middleware;
using SearchForYouApi.Service;

namespace SearchForYouApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
       // 添加 CORS 服务
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddScoped<ISystemService, SystemService>();
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else 
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
        app.UseMiddleware<IPCheck>();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors("AllowAll");
        app.UseAuthorization();

        // 使用 API 路由方式
        app.MapControllers();

        app.Run();
    }
}