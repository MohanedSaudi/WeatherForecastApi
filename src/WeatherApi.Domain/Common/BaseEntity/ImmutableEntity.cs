
namespace WeatherApi.Domain.Common.BaseEntity;
public abstract class ImmutableEntity : IAuditableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "Unknown";
}