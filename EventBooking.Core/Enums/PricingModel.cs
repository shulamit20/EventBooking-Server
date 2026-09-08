namespace EventBooking.Core.Enums;

/// <summary>
/// How a service's price is turned into a line total. The server-side price calculator
/// switches on this value, which is why it is an enum and not a lookup row.
/// </summary>
public enum PricingModel
{
    /// <summary><c>Price</c> is a flat amount per unit ordered (line total = Price × Quantity).</summary>
    Flat = 0,

    /// <summary><c>Price</c> is per guest (line total = Price × GuestCount).</summary>
    PerGuest = 1
}
