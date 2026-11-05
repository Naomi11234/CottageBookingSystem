namespace CottageBookingSystem.Service.Auth;

public sealed class JwtSettings(string issuer, string audience, string signingKey)
{
    public string Issuer { get; } = issuer;
    public string Audience { get; } = audience;
    public string SigningKey { get; } = signingKey;
    public int LifetimeMinutes { get; } = 60;
}
