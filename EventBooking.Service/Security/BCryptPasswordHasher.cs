using EventBooking.Core.Interfaces.Security;

namespace EventBooking.Service.Security;

/// <summary>
/// BCrypt password hashing. The work factor sets how slow one hash is to compute
/// (higher = safer against brute force, slower login). 12 is a common 2020s default.
/// BCrypt embeds the salt and work factor inside the hash string, so <see cref="Verify"/>
/// needs nothing but the stored hash.
/// </summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}
