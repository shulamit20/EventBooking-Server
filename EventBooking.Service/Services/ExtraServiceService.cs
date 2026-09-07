using AutoMapper;
using EventBooking.Core.Common;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;
using EventBooking.Core.Entities;
using EventBooking.Core.Interfaces;
using EventBooking.Core.Interfaces.Repositories;
using EventBooking.Core.Interfaces.Services;

namespace EventBooking.Service.Services;

public class ExtraServiceService : IExtraServiceService
{
    private readonly IExtraServiceRepository _extras;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ExtraServiceService(IExtraServiceRepository extras, IUnitOfWork uow, IMapper mapper)
    {
        _extras = extras;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ExtraServiceResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _extras.GetAllAsync(ct);
        return _mapper.Map<List<ExtraServiceResponse>>(items);
    }

    public async Task<Result<ExtraServiceResponse>> CreateAsync(CreateExtraServiceRequest request, CancellationToken ct = default)
    {
        var service = _mapper.Map<ExtraService>(request);

        await _extras.AddAsync(service, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<ExtraServiceResponse>.Ok(_mapper.Map<ExtraServiceResponse>(service));
    }
}
