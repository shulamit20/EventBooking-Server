using EventBooking.Core.Common;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;
using EventBooking.Core.Enums;
using EventBooking.Core.Interfaces.Repositories;
using EventBooking.Core.Interfaces.Services;

namespace EventBooking.Service.Services;

public class PriceCalculationService : IPriceCalculationService
{
    private readonly IHallSlotRepository _slots;
    private readonly ICateringMenuRepository _catering;
    private readonly IExtraServiceRepository _extras;

    public PriceCalculationService(
        IHallSlotRepository slots,
        ICateringMenuRepository catering,
        IExtraServiceRepository extras)
    {
        _slots = slots;
        _catering = catering;
        _extras = extras;
    }

    public async Task<Result<PriceBreakdownResponse>> CalculateAsync(
        int hallSlotId,
        int guestCount,
        int? cateringMenuId,
        IReadOnlyList<BookingExtraServiceRequest> services,
        CancellationToken ct = default)
    {
        var slot = await _slots.GetByIdWithHallAsync(hallSlotId, ct);
        if (slot is null)
            return Result<PriceBreakdownResponse>.NotFound($"Hall slot {hallSlotId} was not found.");

        // --- catering (per guest) ---
        decimal cateringPerGuest = 0m, cateringTotal = 0m;
        string? cateringName = null;
        if (cateringMenuId is int menuId)
        {
            var menu = await _catering.GetByIdAsync(menuId, ct);
            if (menu is null || !menu.IsActive)
                return Result<PriceBreakdownResponse>.Invalid($"Catering menu {menuId} is not available.");

            cateringName = menu.Name;
            cateringPerGuest = menu.PricePerGuest;
            cateringTotal = menu.PricePerGuest * guestCount;
        }

        // --- extra services ---
        var requestedIds = services.Select(s => s.ExtraServiceId).Distinct().ToList();
        if (requestedIds.Count != services.Count)
            return Result<PriceBreakdownResponse>.Invalid("The same extra service is listed more than once.");

        var loaded = requestedIds.Count == 0
            ? new Dictionary<int, Core.Entities.ExtraService>()
            : (await _extras.GetByIdsAsync(requestedIds, ct)).ToDictionary(e => e.Id);

        if (loaded.Count != requestedIds.Count)
            return Result<PriceBreakdownResponse>.Invalid("One or more extra services do not exist.");

        var lines = services.Select(s =>
        {
            var svc = loaded[s.ExtraServiceId];
            var lineTotal = svc.Pricing == PricingModel.PerGuest
                ? svc.Price * guestCount
                : svc.Price * s.Quantity;

            return new PriceLineResponse
            {
                ExtraServiceId = svc.Id,
                Name = svc.Name,
                Pricing = svc.Pricing.ToString(),
                UnitPrice = svc.Price,
                Quantity = s.Quantity,
                LineTotal = lineTotal,
            };
        }).ToList();

        var total = slot.BasePrice + cateringTotal + lines.Sum(l => l.LineTotal);

        return Result<PriceBreakdownResponse>.Ok(new PriceBreakdownResponse
        {
            VenueBase = slot.BasePrice,
            GuestCount = guestCount,
            CateringMenuId = cateringMenuId,
            CateringMenuName = cateringName,
            CateringPricePerGuest = cateringPerGuest,
            CateringTotal = cateringTotal,
            ServiceLines = lines,
            Total = total,
        });
    }
}
