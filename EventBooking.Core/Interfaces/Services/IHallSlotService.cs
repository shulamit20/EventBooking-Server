using EventBooking.Core.Common;
using EventBooking.Core.DTOs.Common;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;

namespace EventBooking.Core.Interfaces.Services;

public interface IHallSlotService
{
    /// <summary>Browse slots: filter by hall/date/status, sort, paginate.</summary>
    Task<PagedResult<HallSlotResponse>> GetPagedAsync(HallSlotQuery query, CancellationToken ct = default);

    Task<Result<HallSlotResponse>> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Manager only. Invalid if the hall is missing or the (hall, date, shift) slot already exists.</summary>
    Task<Result<HallSlotResponse>> CreateAsync(CreateHallSlotRequest request, CancellationToken ct = default);
}
