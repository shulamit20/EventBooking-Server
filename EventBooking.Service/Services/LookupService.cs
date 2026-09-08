using AutoMapper;
using EventBooking.Core.DTOs.Responses;
using EventBooking.Core.Interfaces.Repositories;
using EventBooking.Core.Interfaces.Services;

namespace EventBooking.Service.Services;

public class LookupService : ILookupService
{
    private readonly ILookupRepository _lookups;
    private readonly IMapper _mapper;

    public LookupService(ILookupRepository lookups, IMapper mapper)
    {
        _lookups = lookups;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<EventTypeResponse>> GetEventTypesAsync(CancellationToken ct = default)
    {
        var types = await _lookups.GetEventTypesAsync(ct);
        return _mapper.Map<List<EventTypeResponse>>(types);
    }

    public async Task<IReadOnlyList<ServiceCategoryResponse>> GetServiceCategoriesAsync(CancellationToken ct = default)
    {
        var categories = await _lookups.GetServiceCategoriesAsync(ct);
        return _mapper.Map<List<ServiceCategoryResponse>>(categories);
    }
}
