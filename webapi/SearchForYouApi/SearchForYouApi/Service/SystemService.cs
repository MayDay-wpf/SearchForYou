using COSXML;
using COSXML.Auth;
using COSXML.CosException;
using COSXML.Model.Object;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SearchForYouApi.Dtos;
using SearchForYouApi.Interface;

namespace SearchForYouApi.Service;

public class SystemService: ISystemService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static string region;
    private static string secretId;
    private static string secretKey;
    private static string bucket;
    
    public SystemService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public UploadImageRes UploadImage(IFormFile file, out string msg)
    {
        msg = string.Empty;
        var imageHost = _configuration.GetValue<string>("ImageHost");
        var result = new UploadImageRes();
        try
        {
            // 检查文件是否为空
            if (file == null || file.Length == 0)
            {
                msg = "没有收到文件";
                return result;
            }

            // 检查是否是图片文件
            if (!file.ContentType.StartsWith("image/"))
            {
                msg = "只允许上传图片文件";
                return result;
            }

            // 检查文件大小（例如限制为5MB）
            const int maxFileSize = 5 * 1024 * 1024;
            if (file.Length > maxFileSize)
            {
                msg = "文件大小超过限制";
                return result;
            }

            // 允许的图片格式
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                msg = "不支持的图片格式";
                return result;
            }

            // 获取wwwroot路径
            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            
            // 构建年月日目录路径
            var today = DateTime.Now;
            var relativePath = Path.Combine("upload", "images", 
                today.ToString("yyyy"), 
                today.ToString("MM"), 
                today.ToString("dd"));
            
            var uploadPath = Path.Combine(webRootPath, relativePath);

            // 创建目录（如果不存在）
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // 生成新的文件名（使用GUID避免重复）
            var newFileName = $"{Guid.NewGuid()}{fileExtension}";
            
            // 完整的文件保存路径
            var filePath = Path.Combine(uploadPath, newFileName);

            // 保存文件
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            // 构建可访问的URL
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            result.Url = $"{baseUrl}/{relativePath.Replace("\\", "/")}/{newFileName}";
            if (imageHost == "COS")
            {
                string key = $"searchforyou/images/{DateTime.Now.ToString("yyyyMMdd")}/{newFileName}";
                result.Url = PutObject(key, filePath, newFileName);
            }
            msg = "上传成功";
        }
        catch (Exception ex)
        {
            msg = $"上传失败：{ex.Message}";
        }
        return result;
    }

    private CosXml CreateCosXml()
    {
        region= _configuration.GetValue<string>("COS:Region");
        secretId= _configuration.GetValue<string>("COS:SecretId");
        secretKey= _configuration.GetValue<string>("COS:SecretKey");
        bucket= _configuration.GetValue<string>("COS:Bucket");
        long durationSecond = 600;
        CosXmlConfig config = new CosXmlConfig.Builder()
            .SetRegion(region) // 设置默认的区域
            .Build();

        QCloudCredentialProvider qCloudCredentialProvider = new DefaultQCloudCredentialProvider(secretId, secretKey, durationSecond);
        return new CosXmlServer(config, qCloudCredentialProvider);
    }

    public string PutObject(string key, string srcPath, string fileName)
    {
        string result = string.Empty;
        try
        {
            CosXml cosXml = CreateCosXml();
            PutObjectRequest request = new PutObjectRequest(bucket, key, srcPath);
            var res = cosXml.PutObject(request);
            result = $"https://{bucket}.cos.{region}.myqcloud.com/{key}";
            //删除本地文件
            DeleteFile(srcPath);
                
        }
        catch (CosClientException clientEx)
        {
        }
        catch (CosServerException serverEx)
        {
        }
        return result;
    }
    public bool DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return true;
        }

        //文件不存在就是删除
        return true;
    }
}
