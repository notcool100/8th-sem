using NtbEvent.Domain.Common;
namespace NtbEvent.Domain.Entities;
public sealed class Tags:BaseEntity
{
    public string Name { get; set; } = string.Empty;
}