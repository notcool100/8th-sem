namespace NtbEvent.Domain.Entities;

public sealed class TagsJn
{
    public long Id { get; set; }
    public long TagId { get; set; }
    public string Action { get; set; } = string.Empty;  // INSERT | UPDATE | DELETE
    public string? OldName { get; set; }
    public string? NewName { get; set; }
    public DateTime ChangedAtUtc { get; set; }
}
