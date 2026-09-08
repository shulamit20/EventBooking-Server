using EventBooking.Core.Common;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.DTOs.Responses;
using EventBooking.Core.Entities;
using EventBooking.Core.Enums;
using EventBooking.Core.Interfaces;
using EventBooking.Core.Interfaces.Repositories;
using EventBooking.Service.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EventBooking.Tests.Services;

/// <summary>
/// Unit tests for the competed operation, <see cref="BookingService.CreateAsync"/>.
/// Every dependency is mocked; only the service's own decision logic is under test.
/// </summary>
public class BookingServiceTests
{
    private readonly Mock<IBookingRepository> _bookings = new();
    private readonly Mock<IHallSlotRepository> _slots = new();
    private readonly Mock<IExtraServiceRepository> _extras = new();
    private readonly Mock<ILookupRepository> _lookups = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IMapper> _mapper = new();

    private BookingService CreateSut() => new(
        _bookings.Object, _slots.Object, _extras.Object, _lookups.Object, _uow.Object, _mapper.Object,
        NullLogger<BookingService>.Instance);

    public BookingServiceTests()
    {
        // Event type 1 exists unless a test overrides it.
        _lookups.Setup(l => l.EventTypeExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private static HallSlot AvailableSlot(int id = 5) =>
        new() { Id = id, HallId = 2, Date = new DateTime(2026, 10, 1), Shift = ShiftType.Evening, BasePrice = 8000m, Status = SlotStatus.Available };

    private static CreateBookingRequest Request(int slotId = 5) => new()
    {
        HallSlotId = slotId,
        EventTypeId = 1,
        HostName = "Cohen",
        GuestCount = 150,
        ExtraServices = new()
    };

    private readonly Guid _user = Guid.NewGuid();

    // ---- success ----

    [Fact]
    public async Task CreateAsync_WhenSlotIsAvailable_TakesTheSlotAndSavesOnce()
    {
        var slot = AvailableSlot();
        _slots.Setup(r => r.GetByIdAsync(slot.Id, It.IsAny<CancellationToken>())).ReturnsAsync(slot);
        _bookings.Setup(r => r.HasActiveBookingForSlotAsync(slot.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _bookings.Setup(r => r.GetDetailedByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Booking());
        _mapper.Setup(m => m.Map<BookingResponse>(It.IsAny<Booking>())).Returns(new BookingResponse { Id = 42 });

        var result = await CreateSut().CreateAsync(Request(slot.Id), _user);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value!.Id);
        Assert.Equal(SlotStatus.Booked, slot.Status);
        _bookings.Verify(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---- rejection: business state ----

    [Fact]
    public async Task CreateAsync_WhenSlotIsNotAvailable_ReturnsConflictAndNeverSaves()
    {
        var slot = AvailableSlot();
        slot.Status = SlotStatus.Booked;
        _slots.Setup(r => r.GetByIdAsync(slot.Id, It.IsAny<CancellationToken>())).ReturnsAsync(slot);

        var result = await CreateSut().CreateAsync(Request(slot.Id), _user);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        _bookings.Verify(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenSlotAlreadyHasAnActiveBooking_ReturnsConflict()
    {
        var slot = AvailableSlot();
        _slots.Setup(r => r.GetByIdAsync(slot.Id, It.IsAny<CancellationToken>())).ReturnsAsync(slot);
        _bookings.Setup(r => r.HasActiveBookingForSlotAsync(slot.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await CreateSut().CreateAsync(Request(slot.Id), _user);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---- rejection: lost the concurrency race ----

    [Fact]
    public async Task CreateAsync_WhenSaveHitsAConcurrencyConflict_ReturnsConflict()
    {
        var slot = AvailableSlot();
        _slots.Setup(r => r.GetByIdAsync(slot.Id, It.IsAny<CancellationToken>())).ReturnsAsync(slot);
        _bookings.Setup(r => r.HasActiveBookingForSlotAsync(slot.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        var result = await CreateSut().CreateAsync(Request(slot.Id), _user);

        Assert.Equal(ResultStatus.Conflict, result.Status);
    }

    // ---- other guards ----

    [Fact]
    public async Task CreateAsync_WhenSlotDoesNotExist_ReturnsNotFound()
    {
        _slots.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync((HallSlot?)null);

        var result = await CreateSut().CreateAsync(Request(), _user);

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task CreateAsync_WhenARequestedExtraServiceDoesNotExist_ReturnsInvalid()
    {
        var slot = AvailableSlot();
        _slots.Setup(r => r.GetByIdAsync(slot.Id, It.IsAny<CancellationToken>())).ReturnsAsync(slot);
        _bookings.Setup(r => r.HasActiveBookingForSlotAsync(slot.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _extras.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExtraService>());

        var request = Request(slot.Id);
        request.ExtraServices.Add(new BookingExtraServiceRequest { ExtraServiceId = 99, Quantity = 1 });

        var result = await CreateSut().CreateAsync(request, _user);

        Assert.Equal(ResultStatus.Invalid, result.Status);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
