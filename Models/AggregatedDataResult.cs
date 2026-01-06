using Agile_Actors_Assignment.DTOs.External;

namespace Agile_Actors_Assignment.Models
{
    public class AggregatedDataResult
    {
        public PlaceToCoordinatesResponse Place { get; set; } = default!;
        public WeatherApiResponse Weather { get; set; } = default!;
        public NewsApiResponse News { get; set; } = default!;
    }
}
