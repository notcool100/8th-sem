using System.ComponentModel.DataAnnotations;

namespace NtbEvent.Application.Users.Dtos;

public sealed class CreateUserRequest
{
    [Required]
    [MinLength(3)]
    [MaxLength(160)]
    public string FullName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; init; } = string.Empty;

    [MaxLength(120)]
    public string? Department { get; init; }

    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public string Password { get; init; } = string.Empty;

    public List<SetPermissionRequest> Permissions { get; init; } = [];
}
