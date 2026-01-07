using Agile_Actors_Assignment.DTOs.External;
using Agile_Actors_Assignment.Interfaces;
using Agile_Actors_Assignment.Models;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;

namespace Agile_Actors_Assignment.Services
{
    public class PlaceToCoordsClient 
    {
        private readonly string externalApiOptionsName = "PlaceToCoordinates";

        private readonly HttpClient _client;
        private readonly IApiStatistics _apiStatistics;
        private readonly ExternalApiOptions _options;
        private readonly ILogger<PlaceToCoordsClient> _logger;

        public PlaceToCoordsClient(HttpClient client, IOptionsMonitor<ExternalApiOptions> options, IApiStatistics apiStatistics, ILogger<PlaceToCoordsClient> logger)
        {
            _client = client;
            _apiStatistics = apiStatistics;
            _options = options.Get(externalApiOptionsName);
            _logger = logger;
        }

        public async Task<BasicDataResponse<PlaceToCoordinatesResponse>> GetCoordinatesAsync(string placeName)
        {
            if (string.IsNullOrWhiteSpace(placeName))
            {
                return BasicDataResponse<PlaceToCoordinatesResponse>.Fail("Place name cannot be null or empty.");
            }

            var sw = Stopwatch.StartNew();
            HttpResponseMessage response = null;

            try
            {
                _logger.LogInformation("> Fetching coordinates for place: {PlaceName}", placeName);

                response = await _client.GetAsync($"direct?q={placeName}&limit=1&appid={_options.ApiKey}");
                sw.Stop();

                _apiStatistics.RecordRequest(externalApiOptionsName, sw.ElapsedMilliseconds, response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"> PlaceToCoordinates API returned status code: {response.StatusCode} for place: {placeName}");       
                    
                    return BasicDataResponse<PlaceToCoordinatesResponse>.Fail(
                        $"> PlaceToCoordinates API returned error status: {response.StatusCode}",
                        response.StatusCode.ToString());
                }

                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(content) || content == "[]")
                {
                    _logger.LogWarning($"> No coordinates found for place: {placeName}");
                    return BasicDataResponse<PlaceToCoordinatesResponse>.Fail($"Location '{placeName}' not found.");
                }

                var coordinates = JsonSerializer.Deserialize<List<PlaceToCoordinatesResponse>>(content);

                var result = coordinates?.FirstOrDefault();

                if (result == null)
                {
                    _logger.LogWarning($"> Place not found: {placeName}");
                    return BasicDataResponse<PlaceToCoordinatesResponse>.Fail($"Location '{placeName}' not found.");
                }

                _logger.LogInformation($"> Successfully retrieved coordinates for {placeName}: Lat={result.lat}, Lon={result.lon}");

                return BasicDataResponse<PlaceToCoordinatesResponse>.Ok(result);
            }
            catch (HttpRequestException ex)
            {
                sw.Stop();
                if (response != null)
                {
                    _apiStatistics.RecordRequest(externalApiOptionsName, sw.ElapsedMilliseconds, response.StatusCode);
                }
                _logger.LogError(ex, $"HTTP request failed for place: {placeName}");
                return BasicDataResponse<PlaceToCoordinatesResponse>.Fail(ex.Message);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"Failed to deserialize response for place: {placeName}");
                return BasicDataResponse<PlaceToCoordinatesResponse>.Fail(
                    "Failed to parse location data.",
                    "PARSE_ERROR");
            }
            catch (Exception ex)
            {
                sw.Stop();
                if (response != null)
                {
                    _apiStatistics.RecordRequest(externalApiOptionsName, sw.ElapsedMilliseconds, response.StatusCode);
                }
                _logger.LogError(ex, $"Unexpected error while fetching coordinates for place: {placeName}");
                return BasicDataResponse<PlaceToCoordinatesResponse>.Fail(ex.Message);
            }
        }
    }
}



