using AutoMapper;
using EventBooking.Core.Common;
using EventBooking.Core.DTOs.Common;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;
using EventBooking.Core.Entities;
using EventBooking.Core.Enums;
using EventBooking.Core.Interfaces;
using EventBooking.Core.Interfaces.Repositories;
using EventBooking.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventBooking.Service.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookings;
    private readonly IHallSlotRepository _slots;
    private readonly IExtraServiceRepository _extras;
    private readonly ILookupRepository _lookups;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<BookingService> _logger;

    public BookingService(
        IBookingRepository bookings,
        IHallSlotRepository slots,
        IExtraServiceRepository extras,
        ILookupRepository lookups,
        IUnitOfWork uow,
        IMapper mapper,
        ILogger<BookingService> logger)
    {
        _bookings = bookings;
        _slots = slots;
        _extras = extras;
        _lookups = lookups;
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// The competed operation. The availability check and the write live in the SAME transaction
    /// (one <see cref="IUnitOfWork.SaveChangesAsync"/>). Taking the slot updates <c>HallSlot</c>, and
    /// its concurrency token <c>Version</c> goes into the UPDATE ... WHERE. If another request won the
    /// race, zero rows update, EF throws <see cref="DbUpdateConcurrencyException"/> -> 409.
    /// </summary>
    public async Task<Result<BookingResponse>> CreateAsync(
        CreateBookingRequest request, Guid currentUserId, CancellationToken ct = default)
    {
        // 1. Load the slot tracked - we are about to change it.
        var slot = await _slots.GetByIdAsync(request.HallSlotId, ct);
        if (slot is null)
            return Result<BookingResponse>.NotFound($"Hall slot {request.HallSlotId} was not found.");

        if (!await _lookups.EventTypeExistsAsync(request.EventTypeId, ct))
            return Result<BookingResponse>.Invalid($"Event type {request.EventTypeId} does not exist.");

        // 2. Business check (state-dependent -> lives here, not in the controller).
        //    A slot that is already taken is a resource collision -> log at Warning (Part D).
        if (slot.Status != SlotStatus.Available)
        {
            _logger.LogWarning(
                "Booking collision: user {UserId} tried to book slot {SlotId} but its status is {Status}.",
                currentUserId, slot.Id, slot.Status);
            return Result<BookingResponse>.Conflict("This slot is no longer available.");
        }

        if (await _bookings.HasActiveBookingForSlotAsync(slot.Id, ct))
        {
            _logger.LogWarning(
                "Booking collision: slot {SlotId} already has an active booking (user {UserId}).",
                slot.Id, currentUserId);
            return Result<BookingResponse>.Conflict("This slot is no longer available.");
        }

        // 3. Resolve the requested extra services in one query; snapshot their prices.
        var requestedIds = request.ExtraServices.Select(x => x.ExtraServiceId).Distinct().ToList();
        if (requestedIds.Count != request.ExtraServices.Count)
            return Result<BookingResponse>.Invalid("The same extra service is listed more than once.");

        var services = requestedIds.Count == 0
            ? new List<ExtraService>()
            : (await _extras.GetByIdsAsync(requestedIds, ct)).ToList();

        if (services.Count != requestedIds.Count)
            return Result<BookingResponse>.Invalid("One or more extra services do not exist.");

        // 4. Build the booking. Line totals + grand total are computed here from DB prices —
        //    any total the client sent is ignored.
        var lines = request.ExtraServices.Select(line =>
        {
            var svc = services.First(s => s.Id == line.ExtraServiceId);
            var lineTotal = svc.Pricing == PricingModel.PerGuest
                ? svc.Price * request.GuestCount
                : svc.Price * line.Quantity;

            return new BookingExtraService
            {
                ExtraServiceId = svc.Id,
                Quantity = line.Quantity,
                PriceAtBooking = svc.Price,
                LineTotal = lineTotal,
            };
        }).ToList();

        var booking = new Booking
        {
            HallSlotId = slot.Id,
            OwnerUserId = currentUserId,
            EventTypeId = request.EventTypeId,
            HostName = request.HostName,
            GuestCount = request.GuestCount,
            Status = BookingStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow,
            Notes = request.Notes,
            TotalPrice = slot.BasePrice + lines.Sum(l => l.LineTotal),
            BookingExtraServices = lines,
        };

        // 5. Take the slot (mutation on the tracked entity).
        slot.Status = SlotStatus.Booked;
        await _bookings.AddAsync(booking, ct);

        // 6. Commit. Check + write in one round trip.
        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning(
                "Booking conflict: slot {SlotId} was taken by another request while user {UserId} was booking it.",
                slot.Id, currentUserId);
            return Result<BookingResponse>.Conflict("The slot was just taken by someone else. Please choose another.");
        }

        var detailed = await _bookings.GetDetailedByIdAsync(booking.Id, ct);
        return Result<BookingResponse>.Ok(_mapper.Map<BookingResponse>(detailed!));
    }

    public async Task<Result<BookingResponse>> GetByIdAsync(
        int id, Guid currentUserId, bool isManager, CancellationToken ct = default)
    {
        var booking = await _bookings.GetDetailedByIdAsync(id, ct);
        if (booking is null)
            return Result<BookingResponse>.NotFound($"Booking {id} was not found.");

        if (!isManager && booking.OwnerUserId != currentUserId)
            return Result<BookingResponse>.Forbidden("This booking belongs to another user.");

        return Result<BookingResponse>.Ok(_mapper.Map<BookingResponse>(booking));
    }

    public async Task<PagedResult<BookingResponse>> GetMineAsync(
        Guid currentUserId, PageQuery query, CancellationToken ct = default)
    {
        var (items, total) = await _bookings.GetPagedByOwnerAsync(
            currentUserId, query.Page, query.PageSize, ct);

        return Page(items, total, query);
    }

    public async Task<PagedResult<BookingResponse>> GetAllAsync(
        PageQuery query, BookingStatus? status, CancellationToken ct = default)
    {
        var (items, total) = await _bookings.GetPagedAsync(query.Page, query.PageSize, status, ct);
        return Page(items, total, query);
    }

    public async Task<Result<BookingResponse>> CancelAsync(int id, Guid currentUserId, CancellationToken ct = default)
    {
        var booking = await _bookings.GetByIdAsync(id, ct); // tracked
        if (booking is null)
            return Result<BookingResponse>.NotFound($"Booking {id} was not found.");

        if (booking.OwnerUserId != currentUserId)
            return Result<BookingResponse>.Forbidden("This booking belongs to another user.");

        if (booking.Status == BookingStatus.Cancelled)
            return Result<BookingResponse>.Invalid("This booking is already cancelled.");

        booking.Status = BookingStatus.Cancelled;
        if (booking.HallSlotId is int slotId)
            await ReleaseSlotAsync(slotId, ct);

        var conflict = await CommitAsync(id, ct);
        if (conflict is not null) return conflict;

        var detailed = await _bookings.GetDetailedByIdAsync(id, ct);
        return Result<BookingResponse>.Ok(_mapper.Map<BookingResponse>(detailed!));
    }

    public async Task<Result<BookingResponse>> SetStatusAsync(int id, BookingStatus status, CancellationToken ct = default)
    {
        if (status == BookingStatus.Draft)
            return Result<BookingResponse>.Invalid("A booking cannot be moved back to Draft.");

        var booking = await _bookings.GetByIdAsync(id, ct); // tracked
        if (booking is null)
            return Result<BookingResponse>.NotFound($"Booking {id} was not found.");

        switch (status)
        {
            case BookingStatus.Confirmed:
                booking.Status = BookingStatus.Confirmed;
                if (booking.HallSlotId is int confirmSlot)
                    await SetSlotStatusAsync(confirmSlot, SlotStatus.Booked, ct);
                break;

            case BookingStatus.Cancelled:
                booking.Status = BookingStatus.Cancelled;
                if (booking.HallSlotId is int cancelSlot)
                    await ReleaseSlotAsync(cancelSlot, ct);
                break;

            case BookingStatus.Pending:
                booking.Status = BookingStatus.Pending;
                break;
        }

        var conflict = await CommitAsync(id, ct);
        if (conflict is not null) return conflict;

        var detailed = await _bookings.GetDetailedByIdAsync(id, ct);
        return Result<BookingResponse>.Ok(_mapper.Map<BookingResponse>(detailed!));
    }

    // ---- helpers ----

    private PagedResult<BookingResponse> Page(IReadOnlyList<Booking> items, int total, PageQuery query) => new()
    {
        Items = _mapper.Map<List<BookingResponse>>(items),
        Page = query.Page,
        PageSize = query.PageSize,
        TotalCount = total
    };

    private async Task ReleaseSlotAsync(int slotId, CancellationToken ct)
    {
        var slot = await _slots.GetByIdAsync(slotId, ct);
        if (slot is not null && slot.Status == SlotStatus.Booked)
            slot.Status = SlotStatus.Available;
    }

    private async Task SetSlotStatusAsync(int slotId, SlotStatus status, CancellationToken ct)
    {
        var slot = await _slots.GetByIdAsync(slotId, ct);
        if (slot is not null)
            slot.Status = status;
    }

    /// <summary>Commits; returns a Conflict result on a concurrency clash, otherwise null.</summary>
    private async Task<Result<BookingResponse>?> CommitAsync(int bookingId, CancellationToken ct)
    {
        try
        {
            await _uow.SaveChangesAsync(ct);
            return null;
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning("Concurrency conflict while updating booking {BookingId}.", bookingId);
            return Result<BookingResponse>.Conflict("The booking changed in the meantime. Please retry.");
        }
    }
}
