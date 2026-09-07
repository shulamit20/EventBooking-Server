using EventBooking.Core.Entities;
using EventBooking.Core.Enums;

namespace EventBooking.Core.Interfaces.Repositories;

public interface IBookingRepository
{
    /// <summary>Tracked load with its extra-service lines — use for status changes / cancel.</summary>
    Task<Booking?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Read-only load with slot -> hall -> venue and extra services, for a response.</summary>
    Task<Booking?> GetDetailedByIdAsync(int id, CancellationToken ct = default);

    /// <summary>True if the slot already has a booking that is not Cancelled.</summary>
    Task<bool> HasActiveBookingForSlotAsync(int hallSlotId, CancellationToken ct = default);

    Task<(IReadOnlyList<Booking> Items, int TotalCount)> GetPagedByOwnerAsync(
        Guid ownerUserId, int page, int pageSize, CancellationToken ct = default);

    Task<(IReadOnlyList<Booking> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, BookingStatus? status, CancellationToken ct = default);

    Task AddAsync(Booking booking, CancellationToken ct = default);
}
