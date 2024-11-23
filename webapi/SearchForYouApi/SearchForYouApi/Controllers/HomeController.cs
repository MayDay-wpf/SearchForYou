using System.Diagnostics;
using COSXML.Callback;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Managers;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;
using SearchForYouApi.Dtos;
using SearchForYouApi.Interface;
using SearchForYouApi.Models;

namespace SearchForYouApi.Controllers;
[ApiController]
[Route("api/[controller]/[action]")]
public class HomeController : ControllerBase
{
    private readonly ILogger<HomeController> _logger;
    private readonly ISystemService _systemService;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAIService _aiService;
    private readonly ISearchEngineService _searchEngineService;

    public HomeController(ILogger<HomeController> logger, ISystemService systemService,IConfiguration configuration,IHttpContextAccessor httpContextAccessor,IAIService aiService,ISearchEngineService searchEngineService)
    {
        _logger = logger;
        _systemService = systemService;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _aiService = aiService;
        _searchEngineService = searchEngineService;
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
    [HttpPost]
    public async Task GetUserIntent([FromBody] UserIntentRequest request)
    {
        string prompt = $@"
        # Universal Intent Analysis Task
        Role: Multi-Modal Intent Classification System
        Mode: Direct Intent Output

        # Input
        TYPE: {(string.IsNullOrEmpty(request.ImageUrl) ? "Text Only" : "Text + Image")}
        QUERY: {request.Question}

        # Required Output Format
        ANALYZE AND RETURN:
        1. Primary Intent
        2. Secondary Intent (if applicable)
        3. {(string.IsNullOrEmpty(request.ImageUrl) ? "Key Action Words" : "Key Action Words & Visual Elements")}
        4. User Goal
        5. Context/Scenario

        FORMAT RULES:
        - Keep points brief and specific
        - Response MUST be in the SAME LANGUAGE as the input query
        - For text+image: Include visual analysis
        - For text-only: Focus on textual intent
        ";
        var response = Response;
        response.Headers.Add("Content-Type", "text/event-stream;charset=utf-8");
        response.Headers.Add("Cache-Control", "no-cache");
        response.Headers.Add("Connection", "keep-alive");
        var openAiOptions = new OpenAiOptions();
        openAiOptions.ApiKey = _configuration.GetValue<string>("OpenAIAPI:ApiKey");
        openAiOptions.BaseDomain =  _configuration.GetValue<string>("OpenAIAPI:BaseUrl");
        List<ChatMessage> chatMessages= new List<ChatMessage>();
        var visionMessageContent = new List<MessageContent>();
        if (string.IsNullOrEmpty(request.ImageUrl))
        {
            chatMessages.Add(ChatMessage.FromUser(prompt));
        }else
        {
           var imageData = await _systemService.ImgConvertToBase64(request.ImageUrl, true);
           var messageContents = MessageContent.ImageUrlContent(
               imageData,
               StaticValues.ImageStatics.ImageDetailTypes.High
           );
           var textMessageContent = MessageContent.TextContent(prompt);
           visionMessageContent.Add(textMessageContent);
           visionMessageContent.Add(messageContents);
           chatMessages.Add(ChatMessage.FromUser(visionMessageContent));
        }

        var chatCompletionCreate =_systemService.CreateChatCompletionRequest(true);
        chatCompletionCreate.Messages = chatMessages;
       var intant= await _aiService.GetAIResult(response,chatCompletionCreate, new OpenAIService(openAiOptions));
    }

    public async Task<IActionResult> CreateSearchKeywords([FromBody]SearchKeywords request)
    {
        var question=request.Question;
        var intant=request.Intant;
        string prompt = $@"
                # System Role
                - Role: SEO Specialist
                - Context: search_keywords_generation

                # Task Definition
                - Objective: Generate search keywords
                - Format: Space-separated keywords only

                # Input Data
                - User Query: {question}
                - Analyzed Intent: {intant}

                # Output Requirements
                1. Format: Keywords only
                2. Separator: Single space
                3. No explanations needed
                4. **Response Language: Match user query language**

                Note: Return only space-separated keywords without any additional text or formatting
                ";

        var chatCompletionCreate = _systemService.CreateChatCompletionRequest(false);
        var openAiOptions = new OpenAiOptions();
        openAiOptions.ApiKey = _configuration.GetValue<string>("OpenAIAPI:ApiKey");
        openAiOptions.BaseDomain =  _configuration.GetValue<string>("OpenAIAPI:BaseUrl");
        List<ChatMessage> chatMessages= new List<ChatMessage>();
        chatMessages.Add(ChatMessage.FromUser(prompt));
        chatCompletionCreate.Messages = chatMessages;
        var result =await _aiService.GetAIResultUnStream(chatCompletionCreate, new OpenAIService(openAiOptions));
        return Ok(new
        {
          success=!string.IsNullOrEmpty(result.Choices[0].message.Content)?true:false,
          data=result.Choices[0].message.Content,
          msg=string.Empty 
        });
    }

    [HttpPost]
    public async Task<IActionResult> GetSearchEngineResult([FromBody] SearchEngine searchEngine)
    {
        var result = new List<SearchEngineResult>();
        var keywords=searchEngine.Keyword;
        var imageUrl = searchEngine.ImageUrl;
        searchEngine.Engine = searchEngine.Engine.ToLower();
        bool onlyImg = false;
        if(!string.IsNullOrEmpty(imageUrl))
            onlyImg = true;
        switch (searchEngine.Engine)
        {
            case "bing":
                result = await _searchEngineService.BingSearch(keywords, imageUrl,onlyImg );
                break;
            case "serper":
                result = await _searchEngineService.SerperSearch(keywords, imageUrl, onlyImg);
                break;
            default:
                result = await _searchEngineService.GoogleSearch(keywords, imageUrl, onlyImg);
                break;
        }
        return Ok(
            new
            {
                success=true,
                data=result
            });
    }
    
    [HttpPost]
    public async Task GetAIResultByWeb([FromBody] AIResultByWeb request)
    {
        var searchEngineResultList = request.SearchEngineResultList;
        var res = new AIResultByWeb
        {
            SearchEngineResultList = searchEngineResultList,
            ImageUrl = request.ImageUrl,
            Question = request.Question,
            Intant = request.Intant
        };
        string prompt = $@"
                # Assistant Role and Context
                - Role: Search Results Based QA Assistant
                - Context: web_search_based_qa

                # Primary Task
                - Goal: Answer user question based on search results
                - Key Requirements:
                  * Analyze search results comprehensively
                  * Provide accurate and relevant answers
                  * Cite sources when appropriate

                # Input Information
                - User Question: {request.Question}
                - Search Intent: {request.Intant}
                - Search Results: {JsonConvert.SerializeObject(searchEngineResultList)}
                {(!string.IsNullOrEmpty(request.ImageUrl) ? $"- Image URL: {request.ImageUrl}" : "")}

                # Output Requirements
                ## Answer Structure
                1. Direct answer to the question
                2. Supporting evidence from search results
                3. Additional relevant context

                ## Format Guidelines
                - Citations: Required
                - Language Style: Clear and professional
                - **Response Language: Must match the language of user's question**
                ";
        var response = Response;
        response.Headers.Add("Content-Type", "text/event-stream;charset=utf-8");
        response.Headers.Add("Cache-Control", "no-cache");
        response.Headers.Add("Connection", "keep-alive");
        var openAiOptions = new OpenAiOptions();
        openAiOptions.ApiKey = _configuration.GetValue<string>("OpenAIAPI:ApiKey");
        openAiOptions.BaseDomain =  _configuration.GetValue<string>("OpenAIAPI:BaseUrl");
        List<ChatMessage> chatMessages= new List<ChatMessage>();
        var visionMessageContent = new List<MessageContent>();
        if (string.IsNullOrEmpty(request.ImageUrl))
        {
            chatMessages.Add(ChatMessage.FromUser(prompt));
        }else
        {
            var imageData = await _systemService.ImgConvertToBase64(request.ImageUrl, true);
            var messageContents = MessageContent.ImageUrlContent(
                imageData,
                StaticValues.ImageStatics.ImageDetailTypes.High
            );
            var textMessageContent = MessageContent.TextContent(prompt);
            visionMessageContent.Add(textMessageContent);
            visionMessageContent.Add(messageContents);
            chatMessages.Add(ChatMessage.FromUser(visionMessageContent));
        }

        var chatCompletionCreate = _systemService.CreateChatCompletionRequest(true);
        chatCompletionCreate.Messages = chatMessages;
        var aiResult= await _aiService.GetAIResult(response,chatCompletionCreate, new OpenAIService(openAiOptions));
    }
}