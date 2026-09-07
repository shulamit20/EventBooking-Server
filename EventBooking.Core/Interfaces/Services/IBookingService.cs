using EventBooking.Core.Common;
using EventBooking.Core.DTOs.Common;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;
using EventBooking.Core.Enums;

namespace EventBooking.Core.Interfaces.Services;

public interface IBookingService
{
    /// <summary>
    /// The competed operation. Reads the slot, checks it is free, writes the booking and marks the
    /// slot Booked — all in one transaction. Returns Conflict (-> 409) if another request won the race.
    /// </summary>
    Task<Result<BookingResponse>> CreateAsync(CreateBookingRequest request, Guid currentUserId, CancellationToken ct = default);

    /// <summary>Client sees only their own booking; Manager sees any.</summary>
    Task<Result<BookingResponse>> GetByIdAsync(int id, Guid currentUserId, bool isManager, CancellationToken ct = default);

    Task<PagedResult<BookingResponse>> GetMineAsync(Guid currentUserId, PageQuery query, CancellationToken ct = default);

    /// <summary>Manager only. Optionally filter by status.</summary>
    Task<PagedResult<BookingResponse>> GetAllAsync(PageQuery query, BookingStatus? status, CancellationToken ct = default);

    /// <summary>Client cancels their own booking; the slot is released.</summary>
    Task<Result<BookingResponse>> CancelAsync(int id, Guid currentUserId, CancellationToken ct = default);

    /// <summary>Manager moves a booking to Confirmed / Cancelled; the slot follows.</summary>
    Task<Result<BookingResponse>> SetStatusAsync(int id, BookingStatus status, CancellationToken ct = default);
}
