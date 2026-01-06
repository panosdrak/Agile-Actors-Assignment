using Agile_Actors_Assignment.DTOs.External;
using Agile_Actors_Assignment.Interfaces;
using Agile_Actors_Assignment.Models;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net.Http;

namespace Agile_Actors_Assignment.Services
{
    public class NewsApiClient
    {
        private readonly string externalApiOptionsName = "News API";


        private readonly HttpClient _client;
        private readonly IApiStatistics _apiStatistics;
        private readonly ExternalApiOptions _options;
        private readonly ILogger<NewsApiClient> _logger;

        public NewsApiClient(HttpClient client, IOptionsMonitor<ExternalApiOptions> options, IApiStatistics apiStatistics, ILogger<NewsApiClient> logger)
        {
            _client = client;
            _apiStatistics = apiStatistics;
            _options = options.Get(externalApiOptionsName);
            _logger = logger;
        }

        public async Task<BasicDataResponse<NewsApiResponse>> GetNewsAsync(string query)
        {
            var sw = Stopwatch.StartNew();
            HttpResponseMessage response = null;

            try
            {
                _logger.LogInformation($"> Fetching for query : {query}");

                response = await _client.GetAsync($"everything?q={query}&apiKey={_options.ApiKey}");
                sw.Stop();


                _apiStatistics.RecordRequest(externalApiOptionsName, sw.ElapsedMilliseconds, response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"> News API returned status code: {response.StatusCode} for query: {query}");

                    return BasicDataResponse<NewsApiResponse>.Fail(
                        $"News API returned error status: {response.StatusCode}",
                        response.StatusCode.ToString());
                }

                var newsDataString = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(newsDataString))
                {
                    _logger.LogWarning($"> News API returned null response for query: {query}");
                    return BasicDataResponse<NewsApiResponse>.Fail("News API returned empty response.", "EMPTY_RESPONSE");
                }

                var newsData = System.Text.Json.JsonSerializer.Deserialize<NewsApiResponse>(newsDataString);

                if (newsData == null)
                {
                    _logger.LogWarning($"> Failed to deserialize news data for query: {query}");
                    return BasicDataResponse<NewsApiResponse>.Fail("Failed to parse news data.");
                }
                _logger.LogInformation($"> Successfully retrieved news data for query: {query}");
                return BasicDataResponse<NewsApiResponse>.Ok(newsData);
            }
            catch (HttpRequestException ex)
            {
                sw.Stop();
                if (response != null)
                {
                    _apiStatistics.RecordRequest(externalApiOptionsName, sw.ElapsedMilliseconds, response.StatusCode);
                }
                _logger.LogError(ex, $"HTTP request failed for query: {query}");
                return BasicDataResponse<NewsApiResponse>.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                sw.Stop();
                if (response != null)
                {
                    _apiStatistics.RecordRequest(externalApiOptionsName, sw.ElapsedMilliseconds, response.StatusCode);
                }
                _logger.LogError(ex, $"Unexpected error while fetching news for query: {query}");
                return BasicDataResponse<NewsApiResponse>.Fail(ex.Message);
            }
        }
    }
}
