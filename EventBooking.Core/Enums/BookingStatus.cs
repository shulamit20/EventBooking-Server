namespace EventBooking.Core.Enums;

/// <summary>Lifecycle state of a customer's booking.</summary>
public enum BookingStatus
{
    /// <summary>Submitted, waiting for the manager.</summary>
    Pending = 0,

    /// <summary>Approved by the manager.</summary>
    Confirmed = 1,

    /// <summary>Cancelled by the customer or the manager; the slot is released.</summary>
    Cancelled = 2
}
