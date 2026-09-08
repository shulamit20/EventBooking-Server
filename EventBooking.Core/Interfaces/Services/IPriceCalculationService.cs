using EventBooking.Core.Common;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;

namespace EventBooking.Core.Interfaces.Services;

/// <summary>
/// The single place that turns a selection (slot + guests + catering + extra services) into a
/// price. Every amount comes from the database — a total sent by the client is never used.
/// Used both by the "price estimate" endpoint and by <c>BookingService</c> when it saves.
/// </summary>
public interface IPriceCalculationService
{
    Task<Result<PriceBreakdownResponse>> CalculateAsync(
        int hallSlotId,
        int guestCount,
        int? cateringMenuId,
        IReadOnlyList<BookingExtraServiceRequest> services,
        CancellationToken ct = default);
}
