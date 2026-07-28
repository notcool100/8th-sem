namespace NtbEvent.Domain.Entities;

public sealed class NavItem
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
