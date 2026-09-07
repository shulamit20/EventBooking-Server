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
}
