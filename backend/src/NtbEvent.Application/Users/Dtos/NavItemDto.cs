namespace NtbEvent.Application.Users.Dtos;

public sealed class NavItemDto
{
    public int Id { get; init; }
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Section { get; init; } = string.Empty;
    public int SortOrder { get; init; }
}
