using Agile_Actors_Assignment.DTOs.Aggregated;
using Agile_Actors_Assignment.DTOs.External;
using Agile_Actors_Assignment.Interfaces;
using Agile_Actors_Assignment.Models;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Agile_Actors_Assignment.Services
{
    public class WeatherApiClient : IExternalApiClient
    {
        public string externalApiOptionsName => "Weather";

        private readonly HttpClient _client;
        private readonly IApiStatistics _apiStatistics;
        private readonly ExternalApiOptions _options;
        private readonly ILogger<WeatherApiClient> _logger;

        private readonly PlaceToCoordsClient _placeToCoordsClient;

        public WeatherApiClient(HttpClient client, IOptionsMonitor<ExternalApiOptions> options, IApiStatistics apiStatistics, ILogger<WeatherApiClient> logger, PlaceToCoordsClient placeToCoordsClient)
        {
            _client = client;
            _apiStatistics = apiStatistics;
            _options = options.Get(externalApiOptionsName);
            _logger = logger;
            _placeToCoordsClient = placeToCoordsClient;
        }

        public async Task<BasicDataResponse<ExternalApiWrapperDto>> FetchAsync(string locationName, string newsKeyword, CancellationToken ct)
        {
            ExternalApiWrapperDto wrapperDto = new ExternalApiWrapperDto()
            {
                Source = externalApiOptionsName,
            };

            var placeResponse = await _placeToCoordsClient.GetCoordinatesAsync(locationName);
            if (!placeResponse.Success || placeResponse.Data == null)
            {
                _logger.LogWarning("Failed to resolve location {Location}: {Message}", locationName, placeResponse.Message);
                return BasicDataResponse<ExternalApiWrapperDto>.Fail(
                    placeResponse.Message,
                    placeResponse.ErrorCode ?? "LOCATION_LOOKUP_FAILED");
            }

            var sw = Stopwatch.StartNew();
            HttpResponseMessage response = null;

            var lat = placeResponse.Data.lat;
            var lon = placeResponse.Data.lon;

            try
            {

                _logger.LogInformation($"> Fetching weather for coordinates: Lat={lat}, Lon={lon}");

                response = await _client.GetAsync($"onecall?lat={lat}&lon={lon}&exclude=minutely,hourly,daily&appid={_options.ApiKey}");
                sw.Stop();

                _apiStatistics.RecordRequest(externalApiOptionsName, sw.ElapsedMilliseconds, response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"> Weather API returned status code: {response.StatusCode} for coordinates: Lat={lat}, Lon={lon}");

                

                    return BasicDataResponse<ExternalApiWrapperDto>.Fail(
                        $"Weather API returned error status: {response.StatusCode}",
                        response.StatusCode.ToString());
                }

                var weatherDataString = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(weatherDataString))
                {
                    _logger.LogWarning($"> Weather API returned null response for coordinates: Lat={lat}, Lon={lon}");
                    return BasicDataResponse<ExternalApiWrapperDto>.Fail("Weather API returned empty response.", "EMPTY_RESPONSE");
                }

                var weatherData = System.Text.Json.JsonSerializer.Deserialize<WeatherApiResponse>(weatherDataString);

                if (weatherData == null)
                {
                    _logger.LogWarning($"> Failed to deserialize weather data for coordinates: Lat={lat}, Lon={lon}");
                    return BasicDataResponse<ExternalApiWrapperDto>.Fail("Failed to parse weather data.");
                }
                _logger.LogInformation($"> Successfully retrieved weather data for coordinates: Lat={lat}, Lon={lon}");

                wrapperDto = new ExternalApiWrapperDto
                {
                    Source = externalApiOptionsName,
                    Data = weatherData
                };

                return BasicDataResponse<ExternalApiWrapperDto>.Ok(wrapperDto);
            }
            catch (HttpRequestException ex)
            {
                sw.Stop();
                if (response != null)
                {
                    _apiStatistics.RecordRequest(externalApiOptionsName, sw.ElapsedMilliseconds, response.StatusCode);
                }
                _logger.LogError(ex, $"HTTP request failed for coordinates: Lat={lat}, Lon={lon}");
                return BasicDataResponse<ExternalApiWrapperDto>.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                sw.Stop();
                if (response != null)
                {
                    _apiStatistics.RecordRequest(externalApiOptionsName, sw.ElapsedMilliseconds, response.StatusCode);
                }
                _logger.LogError(ex, $"Unexpected error while fetching weather for coordinates: Lat={lat}, Lon={lon}");
                return BasicDataResponse<ExternalApiWrapperDto>.Fail(ex.Message);
            }
        }
    }
}
