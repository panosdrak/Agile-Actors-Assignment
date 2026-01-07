using Agile_Actors_Assignment.DTOs.External;
using Agile_Actors_Assignment.Interfaces;
using Agile_Actors_Assignment.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Agile_Actors_Assignment.Services
{
    public class WeatherStackApiClient
    {
        private readonly string externalApiOptionsName = "WeatherStack API";

        private readonly HttpClient _client;
        private readonly IApiStatistics _apiStatistics;
        private readonly ExternalApiOptions _options;
        private readonly ILogger<WeatherStackApiClient> _logger;

        public WeatherStackApiClient(HttpClient client, IOptionsMonitor<ExternalApiOptions> options, IApiStatistics apiStatistics, ILogger<WeatherStackApiClient> logger)
        {
            _client = client;
            _apiStatistics = apiStatistics;
            _options = options.Get(externalApiOptionsName);
            _logger = logger;
        }

        public async Task<BasicDataResponse<WeatherStackApiResponse>> GetWeatherAsync(string location)
        {
            var sw = Stopwatch.StartNew();
            HttpResponseMessage response = null;

            try
            {
                _logger.LogInformation($"> Fetching weather for location: {location}");

                response = await _client.GetAsync($"current?query={location}&access_key={_options.ApiKey}");
                sw.Stop();

                _apiStatistics.RecordRequest(externalApiOptionsName, sw.ElapsedMilliseconds, response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"> Weather Stack API returned status code: {response.StatusCode} for location: {location}");

                    return BasicDataResponse<WeatherStackApiResponse>.Fail(
                        $"Weather Stack API returned error status: {response.StatusCode}",
                        response.StatusCode.ToString());
                }

                var weatherDataString = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(weatherDataString))
                {
                    _logger.LogWarning($"> Weather Stack API returned null response for location: {location}");
                    return BasicDataResponse<WeatherStackApiResponse>.Fail("Weather Stack API returned empty response.", "EMPTY_RESPONSE");
                }

                var weatherData = JsonConvert.DeserializeObject<WeatherStackApiResponse>(weatherDataString);

                if (weatherData == null)
                {
                    _logger.LogWarning($"> Failed to deserialize weather data for location: {location}");
                    return BasicDataResponse<WeatherStackApiResponse>.Fail("Failed to parse weather data.");
                }
                _logger.LogInformation($"> Successfully retrieved weather data for location: {location}");
                return BasicDataResponse<WeatherStackApiResponse>.Ok(weatherData);
            }
            catch (HttpRequestException ex)
            {
                sw.Stop();
                if (response != null)
                {
                    _apiStatistics.RecordRequest(externalApiOptionsName, sw.ElapsedMilliseconds, response.StatusCode);
                }
                _logger.LogError(ex, $"HTTP request failed for location: {location}");
                return BasicDataResponse<WeatherStackApiResponse>.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                sw.Stop();
                if (response != null)
                {
                    _apiStatistics.RecordRequest(externalApiOptionsName, sw.ElapsedMilliseconds, response.StatusCode);
                }
                _logger.LogError(ex, $"Unexpected error while fetching weather for location: {location}");
                return BasicDataResponse<WeatherStackApiResponse>.Fail(ex.Message);
            }
        }
    }
}
