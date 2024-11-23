using OpenAI.ObjectModels.RequestModels;
using SearchForYouApi.Dtos;

namespace SearchForYouApi.Interface;

public interface ISearchEngineService
{
    Task<List<SearchEngineResult>> GoogleSearch(string query,string imageUrl,bool onlyImg = false);
    Task<List<SearchEngineResult>> BingSearch(string query, string imageUrl, bool onlyImg = false);
    Task<List<SearchEngineResult>> SerperSearch(string query, string imageUrl, bool onlyImg = false);
}