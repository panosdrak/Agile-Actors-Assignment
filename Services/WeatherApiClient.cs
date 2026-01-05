using Agile_Actors_Assignment.DTOs.External;

namespace Agile_Actors_Assignment.Services
{
    public class WeatherApiClient
    {
        private readonly HttpClient _client;

        public WeatherApiClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<WeatherDto> GetWeatherAsync(string city)
        {
            return await _client.GetFromJsonAsync<WeatherDto>($"weather?city={city}");
        }
    }
}
