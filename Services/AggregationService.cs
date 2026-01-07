using Agile_Actors_Assignment.DTOs.Aggregated;
using Agile_Actors_Assignment.DTOs.External;
using Agile_Actors_Assignment.Interfaces;
using Agile_Actors_Assignment.Models;
using Microsoft.Extensions.Logging;
using static Agile_Actors_Assignment.DTOs.External.WeatherStackApiResponse;

namespace Agile_Actors_Assignment.Services
{
    public class AggregationService
    {
  
        private readonly IReadOnlyList<IExternalApiClient> _clients;
        private readonly ILogger<AggregationService> _logger;

        public AggregationService(
            IEnumerable<IExternalApiClient> parallelClients,
            ILogger<AggregationService> logger)
        {
            _clients = parallelClients.ToList();
            _logger = logger;
        }   
        

        public async Task<BasicDataResponse<AggregatedDataResult>> GetAggregatedDataAsync(string locationName, string newsKeyword, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(locationName))
            {
                return BasicDataResponse<AggregatedDataResult>.Fail("Location parameter is required.", "INVALID_LOCATION");
            }

            _logger.LogInformation("> Starting aggregation for location: {Location}", locationName);

         

            var tasks = _clients.Select(c => c.FetchAsync(locationName, newsKeyword, ct));
            BasicDataResponse<ExternalApiWrapperDto>[] results = await Task.WhenAll(tasks);
            var combined = results.ToList();

            var aggregatedResult = new AggregatedDataResult();

            aggregatedResult.ExternalData = combined
                .Where(r => r.Data != null)
                .Select(r => r.Data)
                .ToList();


            _logger.LogInformation("Aggregation completed for location: {Location}", locationName);

            return BasicDataResponse<AggregatedDataResult>.Ok(aggregatedResult, "Aggregated data retrieved successfully");
        }
    }
}
