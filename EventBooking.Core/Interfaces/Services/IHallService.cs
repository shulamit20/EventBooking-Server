using EventBooking.Core.Common;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;

namespace EventBooking.Core.Interfaces.Services;

public interface IHallService
{
    Task<Result<HallResponse>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<IReadOnlyList<HallResponse>>> GetByVenueAsync(int venueId, CancellationToken ct = default);
    Task<Result<HallResponse>> CreateAsync(CreateHallRequest request, CancellationToken ct = default);
}
