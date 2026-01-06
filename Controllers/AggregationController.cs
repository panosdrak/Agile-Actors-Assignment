using Agile_Actors_Assignment.Interfaces;
using Agile_Actors_Assignment.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Agile_Actors_Assignment.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AggregationController : ControllerBase
    {
        private readonly IAggregationService _aggregationService;
        private readonly ILogger<AggregationController> _logger;
        private readonly IApiStatistics _apiStatistics;
        private readonly IOptionsMonitor<ExternalApiOptions> _externalApiOptionsMonitor;

        public AggregationController(
            IAggregationService aggregationService,
            ILogger<AggregationController> logger,
            IApiStatistics apiStatistics,
            IOptionsMonitor<ExternalApiOptions> optionsMonitor)
        {
            _aggregationService = aggregationService;
            _logger = logger;
            _apiStatistics = apiStatistics;
            _externalApiOptionsMonitor = optionsMonitor;
        }

        [HttpGet("aggregate", Name = "GetAggregatedData")]
        public async Task<IActionResult> GetAggregatedData([FromQuery] string location, [FromQuery] string? newsQuery = null)
        {
            var response = await _aggregationService.GetAggregatedDataAsync(location, newsQuery);

            if (!response.Success)
            {
                _logger.LogWarning(
                    "Aggregation failed for location {Location}. ErrorCode: {ErrorCode}, Message: {Message}",
                    location,
                    response.ErrorCode,
                    response.Message);

                return response.ErrorCode switch
                {
                    "INVALID_LOCATION" => BadRequest(new { error = response.Message, errorCode = response.ErrorCode }),
                    "LOCATION_NOT_FOUND" => NotFound(new { error = response.Message }),
                    "API_UNAVAILABLE" => StatusCode(503, new { error = response.Message }),
                    _ => StatusCode(500, new { error = response.Message, errorCode = response.ErrorCode })
                };
            }

            _logger.LogInformation("Successfully aggregated data for location: {Location}", location);

            return Ok(new
            {
                success = true,
                data = response.Data
            });
        }

        [HttpGet("stats", Name = "Statistics")]
        public IActionResult GetStatistics()
        {
            Dictionary<string, AggregatedStatisticsPerAPI> stats = new Dictionary<string, AggregatedStatisticsPerAPI>();

            var apiStats = _apiStatistics.GetStatistics();

            if (apiStats != null)
            {
                if (apiStats.Count == 0)
                {
                    return Ok("No statistics available yet.");
                }

                foreach (var apiStat in apiStats)
                {
                    var API_options = _externalApiOptionsMonitor.Get(apiStat.Key);

                    var aggregatedStat = new AggregatedStatisticsPerAPI
                    {
                        totalRequestCount = apiStat.Value.Count,
                        averageResponseTimeMs = apiStat.Value.Average(x => x.ResponseTimeMs),
                        groupedByPerformanceDictionary = apiStat.Value
                            .GroupBy(x =>
                            {
                                if (x.ResponseTimeMs <= API_options.fastResponseTimeMsThreshold)
                                {
                                    return "fast";
                                }

                                if (x.ResponseTimeMs <= API_options.averageResponseTimeMsThreshold)
                                {
                                    return "average";
                                }

                                return "slow";
                            })
                            .ToDictionary(g => g.Key, g => g.Count())
                    };
                    stats[apiStat.Key] = aggregatedStat;
                }
                return Ok(stats);
            }
            else
            {
                return StatusCode(500, new { error = "Failed to retrieve API statistics." });
            }
        }
    }
}
