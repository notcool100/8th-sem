using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NtbEvent.Application.Auth.Dtos;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Domain.Entities;
using NtbEvent.Infrastructure.Configuration;

namespace NtbEvent.Infrastructure.Services;

public sealed class TokenService : ITokenService
{
    private readonly JwtOptions _jwtOptions;

    public TokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public AccessTokenResult GenerateAccessToken(User user)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenLifetimeMinutes);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.FullName),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("department", user.Department)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAtUtc,
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return new AccessTokenResult
        {
            Token = tokenHandler.WriteToken(token),
            ExpiresAtUtc = expiresAtUtc
        };
    }

    public RefreshTokenResult GenerateRefreshToken(string? tokenFamily = null)
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(64);
        var token = WebEncoders.Base64UrlEncode(tokenBytes);

        return new RefreshTokenResult
        {
            Token = token,
            TokenHash = ComputeRefreshTokenHash(token),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenLifetimeDays),
            TokenFamily = string.IsNullOrWhiteSpace(tokenFamily)
                ? Guid.NewGuid().ToString("N")
                : tokenFamily
        };
    }

    public string ComputeRefreshTokenHash(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
