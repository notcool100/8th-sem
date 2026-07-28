using System.ComponentModel.DataAnnotations;

namespace NtbEvent.Application.Auth.Dtos;

public sealed class RefreshSessionRequest
{
    [Required]
    public string RefreshToken { get; init; } = string.Empty;
}
