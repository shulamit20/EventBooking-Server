using AutoMapper;
using EventBooking.Core.Common;
using EventBooking.Core.DTOs.Common;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;
using EventBooking.Core.Entities;
using EventBooking.Core.Interfaces;
using EventBooking.Core.Interfaces.Repositories;
using EventBooking.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Service.Services;

public class VenueService : IVenueService
{
    private readonly IVenueRepository _venues;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public VenueService(IVenueRepository venues, IUnitOfWork uow, IMapper mapper)
    {
        _venues = venues;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<VenueResponse>> GetPagedAsync(PageQuery query, CancellationToken ct = default)
    {
        var (items, total) = await _venues.GetPagedAsync(query.Page, query.PageSize, ct);

        return new PagedResult<VenueResponse>
        {
            Items = _mapper.Map<List<VenueResponse>>(items),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<Result<VenueResponse>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var venue = await _venues.GetByIdAsync(id, ct);
        return venue is null
            ? Result<VenueResponse>.NotFound($"Venue {id} was not found.")
            : Result<VenueResponse>.Ok(_mapper.Map<VenueResponse>(venue));
    }

    public async Task<Result<VenueResponse>> CreateAsync(
        CreateVenueRequest request, Guid ownerUserId, CancellationToken ct = default)
    {
        var venue = _mapper.Map<Venue>(request);
        venue.OwnerUserId = ownerUserId;   // the manager who created it owns it

        await _venues.AddAsync(venue, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<VenueResponse>.Ok(_mapper.Map<VenueResponse>(venue));
    }

    public async Task<Result<VenueResponse>> UpdateAsync(int id, UpdateVenueRequest request, CancellationToken ct = default)
    {
        var venue = await _venues.GetByIdAsync(id, ct); // tracked
        if (venue is null)
            return Result<VenueResponse>.NotFound($"Venue {id} was not found.");

        _mapper.Map(request, venue); // copies fields onto the tracked entity
        await _uow.SaveChangesAsync(ct);

        return Result<VenueResponse>.Ok(_mapper.Map<VenueResponse>(venue));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var venue = await _venues.GetByIdAsync(id, ct);
        if (venue is null)
            return Result.NotFound($"Venue {id} was not found.");

        _venues.Remove(venue);

        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // FK from Halls -> Venue is Restrict: a venue with halls cannot be removed.
            return Result.Conflict("This venue has halls and cannot be deleted.");
        }

        return Result.Ok();
    }
}
