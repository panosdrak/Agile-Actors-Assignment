namespace Agile_Actors_Assignment.Models
{
    public class ExternalApiOptions
{
        public string BaseUrl { get; set; } = default!;
        public string ApiKey { get; set; } = default!;
        public int fastResponseTimeMsThreshold { get; set; }
        public int averageResponseTimeMsThreshold { get; set; }
    }
}
