using Agile_Actors_Assignment.Models;

namespace Agile_Actors_Assignment.Interfaces
{
    public interface IAggregationService
    {
        Task<BasicDataResponse<AggregatedDataResult>> GetAggregatedDataAsync(string location, string? newsQuery = null);
    }
}
