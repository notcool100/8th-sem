using NtbEvent.Domain.Common;

namespace NtbEvent.Domain.Entities;
public sealed class CatagoriesTags:BaseEntity
{
public long CategoryId { get; set; }
public Categories Category { get; set; } = null!;
public long TagId { get; set; }
public Tags Tag { get; set; } = null!;
}