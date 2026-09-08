namespace EventBooking.Core.Enums;

/// <summary>Lifecycle state of a customer's booking.</summary>
public enum BookingStatus
{
    /// <summary>Being built in the Event Builder — no hall slot taken yet.</summary>
    Draft = 0,

    /// <summary>Submitted, waiting for the manager.</summary>
    Pending = 1,

    /// <summary>Approved by the manager.</summary>
    Confirmed = 2,

    /// <summary>Cancelled by the customer or the manager; the slot is released.</summary>
    Cancelled = 3
}
