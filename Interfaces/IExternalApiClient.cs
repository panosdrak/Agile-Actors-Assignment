using Agile_Actors_Assignment.Models;
using Agile_Actors_Assignment.DTOs.External;

namespace Agile_Actors_Assignment.Interfaces
{
    public interface IExternalApiClient
    {
        string externalApiOptionsName { get; }

        Task<BasicDataResponse<ExternalApiWrapperDto>> FetchAsync(string query, CancellationToken ct);
    }
}
