using System.ComponentModel.DataAnnotations;

namespace NtbEvent.Application.Users.Dtos;

public sealed class UpdateUserRequest
{
    [Required]
    [MinLength(3)]
    [MaxLength(160)]
    public string FullName { get; init; } = string.Empty;

    [MaxLength(120)]
    public string? Department { get; init; }

    public bool IsActive { get; init; }
}
