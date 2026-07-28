using NtbEvent.Domain.Common;

namespace NtbEvent.Domain.Entities;

public sealed class EventTags : BaseEntity
{
    public long EventId { get; set; }
    public Event Event { get; set; } = null!;
    public long TagId { get; set; }
    public Tags Tag { get; set; } = null!;
}