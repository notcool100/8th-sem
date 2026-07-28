using System.ComponentModel.DataAnnotations.Schema;
using NtbEvent.Domain.Common;
using NtbEvent.Domain.Enums;

namespace NtbEvent.Domain.Entities;

public sealed class Categories : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public bool IsHoliday { get; set; } = false;

    public CategoryType Type { get; set; } = CategoryType.Event;

    [NotMapped]
public string[]? Tag { get; set; }

}