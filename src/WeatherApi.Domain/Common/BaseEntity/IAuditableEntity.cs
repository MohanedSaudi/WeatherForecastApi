namespace WeatherApi.Domain.Common.BaseEntity;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    string CreatedBy { get; set; }
}

