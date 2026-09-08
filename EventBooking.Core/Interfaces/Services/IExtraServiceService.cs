using EventBooking.Core.Common;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;

namespace EventBooking.Core.Interfaces.Services;

public interface IExtraServiceService
{
    /// <summary>The full active catalog of add-ons.</summary>
    Task<IReadOnlyList<ExtraServiceResponse>> GetAllAsync(CancellationToken ct = default);

    Task<Result<ExtraServiceResponse>> CreateAsync(
        CreateExtraServiceRequest request, Guid ownerUserId, CancellationToken ct = default);
}
