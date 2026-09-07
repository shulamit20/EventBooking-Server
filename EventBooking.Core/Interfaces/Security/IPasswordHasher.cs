namespace EventBooking.Core.Interfaces.Security;

/// <summary>Hashes and verifies passwords. The implementation lives in the Service layer.</summary>
public interface IPasswordHasher
{
    /// <summary>Returns a salted hash string safe to store in the database.</summary>
    string Hash(string password);

    /// <summary>True if <paramref name="password"/> matches the stored <paramref name="hash"/>.</summary>
    bool Verify(string password, string hash);
}
