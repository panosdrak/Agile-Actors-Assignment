using Agile_Actors_Assignment.DTOs.Aggregated;
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
        private readonly WeatherStackApiClient _weatherStackApiClient;
        private readonly ILogger<AggregationService> _logger;

        public AggregationService(
            PlaceToCoordsClient placeToCoordsClient,
            WeatherApiClient weatherApiClient,
            NewsApiClient newsApiClient,
            ILogger<AggregationService> logger,
            WeatherStackApiClient weatherStackApiClient)
        {
            _placeToCoordsClient = placeToCoordsClient;
            _weatherApiClient = weatherApiClient;
            _newsApiClient = newsApiClient;
            _logger = logger;
            _weatherStackApiClient = weatherStackApiClient;
        }

        public async Task<BasicDataResponse<AggregatedDataResult>> GetAggregatedDataAsync(string locationName)
        {
            if (string.IsNullOrWhiteSpace(locationName))
            {
                return BasicDataResponse<AggregatedDataResult>.Fail("Location parameter is required.", "INVALID_LOCATION");
            }

            _logger.LogInformation("> Starting aggregation for location: {Location}", locationName);

            var placeResponse = await _placeToCoordsClient.GetCoordinatesAsync(locationName);
            if (!placeResponse.Success || placeResponse.Data == null)
            {
                _logger.LogWarning("Failed to resolve location {Location}: {Message}", locationName , placeResponse.Message);
                return BasicDataResponse<AggregatedDataResult>.Fail(
                    placeResponse.Message,
                    placeResponse.ErrorCode ?? "LOCATION_LOOKUP_FAILED");
            }

            var place = placeResponse.Data;
           

            var weatherTask = _weatherApiClient.GetWeatherAsync(place.lat, place.lon);
            var newsTask = _newsApiClient.GetNewsAsync(locationName);
            var weatherStackTask = _weatherStackApiClient.GetWeatherAsync(locationName);

            await Task.WhenAll(weatherTask, newsTask, weatherStackTask);

            var weatherResult = await weatherTask;
            var newsResult = await newsTask;
            var weatherStackResult = await weatherStackTask;

            if (!weatherResult.Success)
            {
                _logger.LogWarning("Weather data retrieval failed for location {Location}: {Message}", locationName, weatherResult.Message);
                return BasicDataResponse<AggregatedDataResult>.Fail(
                    weatherResult.Message,
                    weatherResult.ErrorCode ?? "WEATHER_QUERY_FAILED");
            }

            if (!newsResult.Success)
            {
                _logger.LogWarning("News data retrieval failed for query {Query}: {Message}", locationName, newsResult.Message);
                return BasicDataResponse<AggregatedDataResult>.Fail(
                    newsResult.Message,
                    newsResult.ErrorCode ?? "NEWS_QUERY_FAILED");
            }

            if (!weatherStackResult.Success)
            {
                _logger.LogWarning("WeatherStack data retrieval failed for location {Location}: {Message}", locationName, weatherStackResult.Message);
                return BasicDataResponse<AggregatedDataResult>.Fail(
                    weatherStackResult.Message,
                    weatherStackResult.ErrorCode ?? "WEATHERSTACK_QUERY_FAILED");
            }

            var aggregatedResult = new AggregatedDataResult
            {
                Place = place,
                Weather = weatherResult.Data!,
                News = newsResult.Data!,
                WeatherStack = weatherStackResult.Data!
            };

            _logger.LogInformation("Aggregation completed for location: {Location}", locationName);

            return BasicDataResponse<AggregatedDataResult>.Ok(aggregatedResult, "Aggregated data retrieved successfully");
        }
    }
}
