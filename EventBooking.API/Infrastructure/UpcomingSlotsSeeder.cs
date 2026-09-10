using EventBooking.Core.DTOs.Requests;
using EventBooking.Core.Interfaces.Services;

namespace EventBooking.API.Infrastructure;

/// <summary>
/// Runs at every startup so the calendar is always browsable without a manual manager step:
/// ensures every hall has an Available slot for every day (Saturday excluded) of the current
/// month and the next two, for all three shifts. Idempotent — cheap after the first run,
/// since <see cref="IHallSlotService.GenerateAsync"/> skips days that already have a slot.
/// </summary>
public static class UpcomingSlotsSeeder
{
    public static async Task EnsureUpcomingSlotsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var slots = scope.ServiceProvider.GetRequiredService<IHallSlotService>();

        var today = DateTime.UtcNow.Date;
        for (var i = 0; i < 3; i++)
        {
            var target = today.AddMonths(i);
            await slots.GenerateAsync(new GenerateHallSlotsRequest
            {
                HallId = null,
                Year = target.Year,
                Month = target.Month,
            });
        }
    }
}
