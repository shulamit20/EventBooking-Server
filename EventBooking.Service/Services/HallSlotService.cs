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

namespace EventBooking.Service.Services;

public class HallSlotService : IHallSlotService
{
    private readonly IHallSlotRepository _slots;
    private readonly IHallRepository _halls;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public HallSlotService(IHallSlotRepository slots, IHallRepository halls, IUnitOfWork uow, IMapper mapper)
    {
        _slots = slots;
        _halls = halls;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<HallSlotResponse>> GetPagedAsync(HallSlotQuery query, CancellationToken ct = default)
    {
        var (items, total) = await _slots.GetPagedAsync(
            query.Page, query.PageSize,
            query.HallId, query.FromDate, query.ToDate, query.Status,
            query.SortBy, query.Desc, ct);

        return new PagedResult<HallSlotResponse>
        {
            Items = _mapper.Map<List<HallSlotResponse>>(items),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<Result<HallSlotResponse>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var slot = await _slots.GetByIdWithHallAsync(id, ct);
        return slot is null
            ? Result<HallSlotResponse>.NotFound($"Hall slot {id} was not found.")
            : Result<HallSlotResponse>.Ok(_mapper.Map<HallSlotResponse>(slot));
    }

    public async Task<Result<HallSlotResponse>> CreateAsync(CreateHallSlotRequest request, CancellationToken ct = default)
    {
        if (!await _halls.ExistsAsync(request.HallId, ct))
            return Result<HallSlotResponse>.Invalid($"Hall {request.HallId} does not exist.");

        var slot = _mapper.Map<HallSlot>(request);
        slot.Date = request.Date.Date;          // calendar date only
        slot.Status = SlotStatus.Available;
        slot.Version = Guid.NewGuid();

        await _slots.AddAsync(slot, ct);

        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Unique index (HallId, Date, Shift).
            return Result<HallSlotResponse>.Conflict("A slot for this hall, date and shift already exists.");
        }

        var created = await _slots.GetByIdWithHallAsync(slot.Id, ct);
        return Result<HallSlotResponse>.Ok(_mapper.Map<HallSlotResponse>(created!));
    }

    public async Task<Result<int>> GenerateAsync(GenerateHallSlotsRequest request, CancellationToken ct = default)
    {
        IReadOnlyList<Hall> halls;
        if (request.HallId is int hallId)
        {
            var hall = await _halls.GetByIdWithVenueAsync(hallId, ct);
            if (hall is null)
                return Result<int>.Invalid($"Hall {hallId} does not exist.");
            halls = new[] { hall };
        }
        else
        {
            halls = await _halls.GetAllAsync(ct);
        }

        var from = new DateTime(request.Year, request.Month, 1);
        var to = from.AddMonths(1).AddDays(-1);

        var created = 0;
        foreach (var hall in halls)
        {
            var existing = await _slots.GetExistingKeysAsync(hall.Id, from, to, ct);

            for (var date = from; date <= to; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Saturday)
                    continue; // Shabbat — venues are closed

                foreach (var shift in Enum.GetValues<ShiftType>())
                {
                    if (existing.Contains((date, shift)))
                        continue; // already has a slot (available, booked or otherwise) — leave it alone

                    await _slots.AddAsync(new HallSlot
                    {
                        HallId = hall.Id,
                        Date = date,
                        Shift = shift,
                        BasePrice = PriceFor(hall, shift),
                        Status = SlotStatus.Available,
                        Version = Guid.NewGuid(),
                    }, ct);
                    created++;
                }
            }
        }

        if (created > 0)
            await _uow.SaveChangesAsync(ct);

        return Result<int>.Ok(created);
    }

    private static decimal PriceFor(Hall hall, ShiftType shift) => shift switch
    {
        ShiftType.Morning => hall.MorningPrice,
        ShiftType.Noon => hall.NoonPrice,
        ShiftType.Evening => hall.EveningPrice,
        _ => hall.EveningPrice,
    };
}
