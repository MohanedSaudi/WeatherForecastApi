namespace WeatherApi.Domain.Common.BaseEntity;

public abstract class MutableEntity : IAuditableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "Unknown";
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}