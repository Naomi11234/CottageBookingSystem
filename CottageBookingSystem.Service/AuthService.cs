using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CottageBookingSystem.Core.DTOs;
using CottageBookingSystem.Core.Entities;
using CottageBookingSystem.Core.Repositories;
using CottageBookingSystem.Core.Security;
using CottageBookingSystem.Service.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace CottageBookingSystem.Service;

public sealed class AuthService(
    IUserRepository userRepository,
    IPasswordHasher<User> passwordHasher,
    JwtSettings jwtSettings) : IAuthService
{
    public async Task<AuthResponseDto?> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRegistration(request);

        var normalizedEmail = NormalizeEmail(request.Email);
        if (await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken) is not null)
        {
            return null;
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            Role = RoleNames.Customer
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        if (!await userRepository.TryAddUserAsync(user, cancellationToken))
        {
            return null;
        }

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponseDto?> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            return null;
        }

        var user = await userRepository.GetByEmailAsync(NormalizeEmail(request.Email), cancellationToken);
        if (user is null || passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password)
            == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return CreateAuthResponse(user);
    }

    private AuthResponseDto CreateAuthResponse(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.LifetimeMinutes);
        var claims = new List<Claim>
        {
            new("UserId", user.Id.ToString()),
            new("Email", user.Email),
            new("Role", user.Role)
        };
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: signingCredentials);

        return new AuthResponseDto
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expiresAt,
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role
        };
    }

    private static void ValidateRegistration(RegisterRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Trim().Length > 150)
        {
            throw new ArgumentException("FullName is required and must be 150 characters or fewer.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Email) || request.Email.Trim().Length > 320 ||
            !System.Net.Mail.MailAddress.TryCreate(request.Email.Trim(), out var address) ||
            !string.Equals(address.Address, request.Email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("A valid email address is required.", nameof(request));
        }

        var password = request.Password;
        if (string.IsNullOrEmpty(password) || password.Length < 12 || password.Length > 128 ||
            !password.Any(char.IsUpper) || !password.Any(char.IsLower) ||
            !password.Any(char.IsDigit) || !password.Any(character => !char.IsLetterOrDigit(character)))
        {
            throw new ArgumentException(
                "Password must be 12-128 characters and include uppercase, lowercase, a digit, and a symbol.",
                nameof(request));
        }
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToUpperInvariant();
    }
}
