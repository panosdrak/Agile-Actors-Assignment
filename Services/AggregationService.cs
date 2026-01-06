using Agile_Actors_Assignment.Interfaces;
using Agile_Actors_Assignment.Models;
using Microsoft.Extensions.Logging;

namespace Agile_Actors_Assignment.Services
{
    public class AggregationService
    {
        private readonly PlaceToCoordsClient _placeToCoordsClient;
        private readonly WeatherApiClient _weatherApiClient;
        private readonly NewsApiClient _newsApiClient;
        private readonly ILogger<AggregationService> _logger;

        public AggregationService(
            PlaceToCoordsClient placeToCoordsClient,
            WeatherApiClient weatherApiClient,
            NewsApiClient newsApiClient,
            ILogger<AggregationService> logger)
        {
            _placeToCoordsClient = placeToCoordsClient;
            _weatherApiClient = weatherApiClient;
            _newsApiClient = newsApiClient;
            _logger = logger;
        }

        public async Task<BasicDataResponse<AggregatedDataResult>> GetAggregatedDataAsync(string location, string? newsQuery = null)
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                return BasicDataResponse<AggregatedDataResult>.Fail("Location parameter is required.", "INVALID_LOCATION");
            }

            _logger.LogInformation("> Starting aggregation for location: {Location}", location);

            var placeResponse = await _placeToCoordsClient.GetCoordinatesAsync(location);
            if (!placeResponse.Success || placeResponse.Data == null)
            {
                _logger.LogWarning("Failed to resolve location {Location}: {Message}", location, placeResponse.Message);
                return BasicDataResponse<AggregatedDataResult>.Fail(
                    placeResponse.Message,
                    placeResponse.ErrorCode ?? "LOCATION_LOOKUP_FAILED");
            }

            var place = placeResponse.Data;
            var resolvedNewsQuery = string.IsNullOrWhiteSpace(newsQuery) ? place.name ?? location : newsQuery;

            var weatherTask = _weatherApiClient.GetWeatherAsync(place.lat, place.lon);
            var newsTask = _newsApiClient.GetNewsAsync(resolvedNewsQuery);

            await Task.WhenAll(weatherTask, newsTask);

            var weatherResult = await weatherTask;
            var newsResult = await newsTask;

            if (!weatherResult.Success)
            {
                _logger.LogWarning("Weather data retrieval failed for location {Location}: {Message}", location, weatherResult.Message);
                return BasicDataResponse<AggregatedDataResult>.Fail(
                    weatherResult.Message,
                    weatherResult.ErrorCode ?? "WEATHER_QUERY_FAILED");
            }

            if (!newsResult.Success)
            {
                _logger.LogWarning("News data retrieval failed for query {Query}: {Message}", resolvedNewsQuery, newsResult.Message);
                return BasicDataResponse<AggregatedDataResult>.Fail(
                    newsResult.Message,
                    newsResult.ErrorCode ?? "NEWS_QUERY_FAILED");
            }

            var aggregatedResult = new AggregatedDataResult
            {
                Place = place,
                Weather = weatherResult.Data!,
                News = newsResult.Data!
            };

            _logger.LogInformation("Aggregation completed for location: {Location}", location);

            return BasicDataResponse<AggregatedDataResult>.Ok(aggregatedResult, "Aggregated data retrieved successfully");
        }
    }
}
