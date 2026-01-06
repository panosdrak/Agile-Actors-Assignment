using System.Collections.Concurrent;
using static Agile_Actors_Assignment.Services.ApiStatistics;

namespace Agile_Actors_Assignment.Interfaces
{
    public interface IApiStatistics
    {
        void RecordRequest(string apiName, float responseTimeMs, System.Net.HttpStatusCode statusCode);
        ConcurrentDictionary<string, List<ApiRequestRecord>> GetStatistics();
    }
}
