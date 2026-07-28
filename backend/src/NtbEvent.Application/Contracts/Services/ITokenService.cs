using NtbEvent.Application.Auth.Dtos;
using NtbEvent.Domain.Entities;

namespace NtbEvent.Application.Contracts.Services;

public interface ITokenService
{
    AccessTokenResult GenerateAccessToken(User user);

    RefreshTokenResult GenerateRefreshToken(string? tokenFamily = null);

    string ComputeRefreshTokenHash(string token);
}
