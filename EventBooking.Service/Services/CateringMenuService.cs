using AutoMapper;
using EventBooking.Core.Common;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;
using EventBooking.Core.Entities;
using EventBooking.Core.Interfaces;
using EventBooking.Core.Interfaces.Repositories;
using EventBooking.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Service.Services;

public class CateringMenuService : ICateringMenuService
{
    private readonly ICateringMenuRepository _menus;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CateringMenuService(ICateringMenuRepository menus, IUnitOfWork uow, IMapper mapper)
    {
        _menus = menus;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CateringMenuResponse>> GetActiveAsync(CancellationToken ct = default) =>
        _mapper.Map<List<CateringMenuResponse>>(await _menus.GetActiveAsync(ct));

    public async Task<IReadOnlyList<CateringMenuResponse>> GetMineAsync(Guid managerId, CancellationToken ct = default) =>
        _mapper.Map<List<CateringMenuResponse>>(await _menus.GetByOwnerAsync(managerId, ct));

    public async Task<Result<CateringMenuResponse>> CreateAsync(
        CreateCateringMenuRequest request, Guid managerId, CancellationToken ct = default)
    {
        var menu = _mapper.Map<CateringMenu>(request);
        menu.OwnerUserId = managerId;

        await _menus.AddAsync(menu, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<CateringMenuResponse>.Ok(_mapper.Map<CateringMenuResponse>(menu));
    }

    public async Task<Result<CateringMenuResponse>> UpdateAsync(
        int id, UpdateCateringMenuRequest request, Guid managerId, CancellationToken ct = default)
    {
        var menu = await _menus.GetByIdAsync(id, ct); // tracked
        if (menu is null)
            return Result<CateringMenuResponse>.NotFound($"Catering menu {id} was not found.");

        if (menu.OwnerUserId != managerId)
            return Result<CateringMenuResponse>.Forbidden("This catering menu belongs to another manager.");

        _mapper.Map(request, menu);
        await _uow.SaveChangesAsync(ct);

        return Result<CateringMenuResponse>.Ok(_mapper.Map<CateringMenuResponse>(menu));
    }

    public async Task<Result> DeleteAsync(int id, Guid managerId, CancellationToken ct = default)
    {
        var menu = await _menus.GetByIdAsync(id, ct);
        if (menu is null)
            return Result.NotFound($"Catering menu {id} was not found.");

        if (menu.OwnerUserId != managerId)
            return Result.Forbidden("This catering menu belongs to another manager.");

        _menus.Remove(menu);

        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // A booking references this menu (FK is Restrict).
            return Result.Conflict("This menu is used by a booking and cannot be deleted. Deactivate it instead.");
        }

        return Result.Ok();
    }
}
