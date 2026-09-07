using AutoMapper;
using EventBooking.Core.Common;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;
using EventBooking.Core.Entities;
using EventBooking.Core.Interfaces;
using EventBooking.Core.Interfaces.Repositories;
using EventBooking.Core.Interfaces.Services;

namespace EventBooking.Service.Services;

public class HallService : IHallService
{
    private readonly IHallRepository _halls;
    private readonly IVenueRepository _venues;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public HallService(IHallRepository halls, IVenueRepository venues, IUnitOfWork uow, IMapper mapper)
    {
        _halls = halls;
        _venues = venues;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<HallResponse>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var hall = await _halls.GetByIdWithVenueAsync(id, ct);
        return hall is null
            ? Result<HallResponse>.NotFound($"Hall {id} was not found.")
            : Result<HallResponse>.Ok(_mapper.Map<HallResponse>(hall));
    }

    public async Task<Result<IReadOnlyList<HallResponse>>> GetByVenueAsync(int venueId, CancellationToken ct = default)
    {
        var venue = await _venues.GetByIdAsync(venueId, ct);
        if (venue is null)
            return Result<IReadOnlyList<HallResponse>>.NotFound($"Venue {venueId} was not found.");

        var halls = await _halls.GetByVenueAsync(venueId, ct);
        return Result<IReadOnlyList<HallResponse>>.Ok(_mapper.Map<List<HallResponse>>(halls));
    }

    public async Task<Result<HallResponse>> CreateAsync(CreateHallRequest request, CancellationToken ct = default)
    {
        var venue = await _venues.GetByIdAsync(request.VenueId, ct);
        if (venue is null)
            return Result<HallResponse>.Invalid($"Venue {request.VenueId} does not exist.");

        var hall = _mapper.Map<Hall>(request);
        await _halls.AddAsync(hall, ct);
        await _uow.SaveChangesAsync(ct);

        // Reload with the venue so the response has VenueName.
        var created = await _halls.GetByIdWithVenueAsync(hall.Id, ct);
        return Result<HallResponse>.Ok(_mapper.Map<HallResponse>(created!));
    }
}
