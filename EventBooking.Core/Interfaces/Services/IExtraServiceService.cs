using EventBooking.Core.Common;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;

namespace EventBooking.Core.Interfaces.Services;

public interface IExtraServiceService
{
    Task<IReadOnlyList<ExtraServiceResponse>> GetAllAsync(CancellationToken ct = default);
    Task<Result<ExtraServiceResponse>> CreateAsync(CreateExtraServiceRequest request, CancellationToken ct = default);
}
