namespace NtbEvent.Application.Categorie.Dtos;

public sealed class CategoriesDto
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty; 

    public string Description { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public string[]? Tag { get; set; }

    public bool IsHoliday { get; set; } = false;

    public string Type { get; set; } = "event";

}