using Agile_Actors_Assignment.DTOs.External;

namespace Agile_Actors_Assignment.DTOs.Aggregated
{
    public class AggregatedDataResult
    {
       public List<ExternalApiWrapperDto> ExternalData { get; set; } = new List<ExternalApiWrapperDto>();
    }
}
