using EventBooking.Core.Common;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;

namespace EventBooking.Core.Interfaces.Services;

public interface ICateringMenuService
{
    /// <summary>Active menus, for customers to browse.</summary>
    Task<IReadOnlyList<CateringMenuResponse>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>Every menu the calling manager owns.</summary>
    Task<IReadOnlyList<CateringMenuResponse>> GetMineAsync(Guid managerId, CancellationToken ct = default);

    Task<Result<CateringMenuResponse>> CreateAsync(
        CreateCateringMenuRequest request, Guid managerId, CancellationToken ct = default);

    /// <summary>Forbidden if the menu belongs to another manager.</summary>
    Task<Result<CateringMenuResponse>> UpdateAsync(
        int id, UpdateCateringMenuRequest request, Guid managerId, CancellationToken ct = default);

    Task<Result> DeleteAsync(int id, Guid managerId, CancellationToken ct = default);
}
