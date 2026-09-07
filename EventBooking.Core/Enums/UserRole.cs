namespace EventBooking.Core.Enums;

/// <summary>Application roles. Drives JWT claims and [Authorize] policies.</summary>
public enum UserRole
{
    Client = 0,
    Manager = 1
}
