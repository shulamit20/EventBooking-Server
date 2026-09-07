using EventBooking.Core.Common;
using EventBooking.Core.DTOs.Common;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;

namespace EventBooking.Core.Interfaces.Services;

/// <summary>Full CRUD over venues (Manager only for writes).</summary>
public interface IVenueService
{
    Task<PagedResult<VenueResponse>> GetPagedAsync(PageQuery query, CancellationToken ct = default);
    Task<Result<VenueResponse>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<VenueResponse>> CreateAsync(CreateVenueRequest request, CancellationToken ct = default);
    Task<Result<VenueResponse>> UpdateAsync(int id, UpdateVenueRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
