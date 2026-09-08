using EventBooking.Core.Common;
using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.Entities;
using EventBooking.Core.Enums;
using EventBooking.Core.Interfaces.Repositories;
using EventBooking.Service.Services;
using Moq;

namespace EventBooking.Tests.Services;

/// <summary>
/// The server-side price calculation (spec §7): total = venue base + catering (per guest) +
/// each extra-service line (flat × qty, or per guest).
/// </summary>
public class PriceCalculationServiceTests
{
    private readonly Mock<IHallSlotRepository> _slots = new();
    private readonly Mock<ICateringMenuRepository> _catering = new();
    private readonly Mock<IExtraServiceRepository> _extras = new();

    private PriceCalculationService CreateSut() => new(_slots.Object, _catering.Object, _extras.Object);

    [Fact]
    public async Task Calculate_AddsVenueBase_CateringPerGuest_AndEachServiceLine()
    {
        _slots.Setup(r => r.GetByIdWithHallAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HallSlot { Id = 1, BasePrice = 8000m });

        _catering.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CateringMenu { Id = 2, Name = "Meat Menu", PricePerGuest = 180m, IsActive = true });

        _extras.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExtraService>
            {
                new() { Id = 10, Name = "Live Band", Price = 3500m, Pricing = PricingModel.Flat },
                new() { Id = 11, Name = "Premium Bar", Price = 90m, Pricing = PricingModel.PerGuest },
            });

        var services = new List<BookingExtraServiceRequest>
        {
            new() { ExtraServiceId = 10, Quantity = 1 },
            new() { ExtraServiceId = 11, Quantity = 1 },
        };

        var result = await CreateSut().CalculateAsync(hallSlotId: 1, guestCount: 300, cateringMenuId: 2, services);

        Assert.True(result.IsSuccess);
        var b = result.Value!;
        Assert.Equal(8000m, b.VenueBase);
        Assert.Equal(300 * 180m, b.CateringTotal);          // 54,000
        Assert.Equal(3500m, b.ServiceLines.Single(l => l.ExtraServiceId == 10).LineTotal);
        Assert.Equal(300 * 90m, b.ServiceLines.Single(l => l.ExtraServiceId == 11).LineTotal); // 27,000
        Assert.Equal(8000m + 54000m + 3500m + 27000m, b.Total);  // 92,500
    }

    [Fact]
    public async Task Calculate_WithNoCateringAndNoServices_IsJustTheVenueBase()
    {
        _slots.Setup(r => r.GetByIdWithHallAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HallSlot { Id = 1, BasePrice = 6000m });

        var result = await CreateSut().CalculateAsync(1, 100, null, new List<BookingExtraServiceRequest>());

        Assert.Equal(6000m, result.Value!.Total);
    }

    [Fact]
    public async Task Calculate_WhenTheSlotIsMissing_ReturnsNotFound()
    {
        _slots.Setup(r => r.GetByIdWithHallAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((HallSlot?)null);

        var result = await CreateSut().CalculateAsync(99, 100, null, new List<BookingExtraServiceRequest>());

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task Calculate_WhenAnExtraServiceDoesNotExist_ReturnsInvalid()
    {
        _slots.Setup(r => r.GetByIdWithHallAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HallSlot { Id = 1, BasePrice = 6000m });
        _extras.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExtraService>()); // requested id not found

        var services = new List<BookingExtraServiceRequest> { new() { ExtraServiceId = 999, Quantity = 1 } };

        var result = await CreateSut().CalculateAsync(1, 100, null, services);

        Assert.Equal(ResultStatus.Invalid, result.Status);
    }
}
