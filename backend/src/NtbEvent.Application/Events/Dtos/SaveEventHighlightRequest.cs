using System.ComponentModel.DataAnnotations;

namespace NtbEvent.Application.Events.Dtos;

public sealed class SaveEventHighlightRequest
{
    [MaxLength(60)]
    public string? Icon { get; set; }

    [Required]
    [MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(400)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Tone { get; set; }
}
