namespace EventBooking.Core.Enums;

/// <summary>Application roles. Drives JWT claims and <c>[Authorize]</c> policies.</summary>
public enum UserRole
{
    /// <summary>Books events and manages their own bookings.</summary>
    Customer = 0,

    /// <summary>Owns venues / services / promotions and manages their own.</summary>
    Manager = 1,

    /// <summary>Full system access.</summary>
    Admin = 2
}
