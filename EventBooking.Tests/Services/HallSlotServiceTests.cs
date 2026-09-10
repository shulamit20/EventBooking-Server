using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.Entities;
using EventBooking.Core.Enums;
using EventBooking.Core.Interfaces;
using EventBooking.Core.Interfaces.Repositories;
using EventBooking.Service.Services;
using AutoMapper;
using Moq;

namespace EventBooking.Tests.Services;

/// <summary>
/// Unit tests for <see cref="HallSlotService.GenerateAsync"/> — the job that fills in a whole
/// month of Available slots per hall so the calendar is always browsable/bookable.
/// </summary>
public class HallSlotServiceTests
{
    private readonly Mock<IHallSlotRepository> _slots = new();
    private readonly Mock<IHallRepository> _halls = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IMapper> _mapper = new();

    private HallSlotService CreateSut() => new(_slots.Object, _halls.Object, _uow.Object, _mapper.Object);

    private static Hall Hall(int id = 1) =>
        new() { Id = id, VenueId = 1, Name = "Test Hall", Capacity = 100, MorningPrice = 1000m, NoonPrice = 2000m, EveningPrice = 3000m };

    [Fact]
    public async Task GenerateAsync_SkipsSaturdays_AndCreatesThreeShiftsForEveryOtherDay()
    {
        // November 2026 has 30 days, 4 of them Saturdays -> 26 bookable days * 3 shifts = 78 slots.
        _halls.Setup(h => h.GetByIdWithVenueAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Hall());
        _slots.Setup(s => s.GetExistingKeysAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<(DateTime, ShiftType)>());

        var sut = CreateSut();
        var result = await sut.GenerateAsync(new GenerateHallSlotsRequest { HallId = 1, Year = 2026, Month = 11 });

        Assert.True(result.IsSuccess);
        Assert.Equal(78, result.Value);
        _slots.Verify(s => s.AddAsync(
            It.Is<HallSlot>(slot => slot.Date.DayOfWeek == DayOfWeek.Saturday),
            It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateAsync_SkipsDayShiftCombinationsThatAlreadyHaveASlot()
    {
        var existing = new HashSet<(DateTime, ShiftType)> { (new DateTime(2026, 11, 2), ShiftType.Evening) };
        _halls.Setup(h => h.GetByIdWithVenueAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Hall());
        _slots.Setup(s => s.GetExistingKeysAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var sut = CreateSut();
        var result = await sut.GenerateAsync(new GenerateHallSlotsRequest { HallId = 1, Year = 2026, Month = 11 });

        Assert.True(result.IsSuccess);
        Assert.Equal(77, result.Value); // one fewer than the clean-slate 78
        _slots.Verify(s => s.AddAsync(
            It.Is<HallSlot>(slot => slot.Date == new DateTime(2026, 11, 2) && slot.Shift == ShiftType.Evening),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GenerateAsync_UsesTheHallsPerShiftPrice()
    {
        _halls.Setup(h => h.GetByIdWithVenueAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Hall());
        _slots.Setup(s => s.GetExistingKeysAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<(DateTime, ShiftType)>());

        var sut = CreateSut();
        await sut.GenerateAsync(new GenerateHallSlotsRequest { HallId = 1, Year = 2026, Month = 11 });

        _slots.Verify(s => s.AddAsync(
            It.Is<HallSlot>(slot => slot.Shift == ShiftType.Morning && slot.BasePrice == 1000m),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _slots.Verify(s => s.AddAsync(
            It.Is<HallSlot>(slot => slot.Shift == ShiftType.Evening && slot.BasePrice == 3000m),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GenerateAsync_UnknownHall_ReturnsInvalid()
    {
        _halls.Setup(h => h.GetByIdWithVenueAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Hall?)null);

        var sut = CreateSut();
        var result = await sut.GenerateAsync(new GenerateHallSlotsRequest { HallId = 999, Year = 2026, Month = 11 });

        Assert.False(result.IsSuccess);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GenerateAsync_NoHallId_GeneratesForEveryHall()
    {
        _halls.Setup(h => h.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { Hall(1), Hall(2) });
        _slots.Setup(s => s.GetExistingKeysAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<(DateTime, ShiftType)>());

        var sut = CreateSut();
        var result = await sut.GenerateAsync(new GenerateHallSlotsRequest { HallId = null, Year = 2026, Month = 11 });

        Assert.True(result.IsSuccess);
        Assert.Equal(156, result.Value); // 78 per hall * 2 halls
    }
}
