using Agile_Actors_Assignment.Interfaces;
using System.Collections.Concurrent;
using System.Net;

namespace Agile_Actors_Assignment.Services
{
    public class ApiStatistics : IApiStatistics
    {
        private readonly ConcurrentDictionary<string, List<ApiRequestRecord>> _statistics = new();

        // Get statistics dictionary
        public ConcurrentDictionary<string, List<ApiRequestRecord>> GetStatistics()
        {
            return _statistics;
        }

        public void RecordRequest(string apiName, float responseTimeMs, HttpStatusCode statusCode)
        {
            var record = new ApiRequestRecord
            {
                ResponseTimeMs = responseTimeMs,
                StatusCode = statusCode
            };

            _statistics.AddOrUpdate(
                apiName,
                _ => new List<ApiRequestRecord> { record },
                (_, list) =>
                {
                    lock (list)
                    {
                        list.Add(record);
                    }
                    return list;
                });
        }

        public class ApiRequestRecord
        {
            public float ResponseTimeMs { get; set; }
            public HttpStatusCode StatusCode { get; set; }
        }
    }
}
