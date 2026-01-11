using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WeatherApi.Domain.Weather;

namespace WeatherApi.Infrastructure.Weather;

public class WeatherCacheConfiguration : IEntityTypeConfiguration<WeatherCache>
{
    public void Configure(EntityTypeBuilder<WeatherCache> builder)
    {
        builder.ToTable("WeatherCaches");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.City).IsRequired().HasMaxLength(200);
        builder.Property(w => w.TemperatureC).IsRequired();
        builder.Property(w => w.TemperatureF).IsRequired();
        builder.Property(w => w.Summary).IsRequired().HasMaxLength(100);
        builder.Property(w => w.Description).IsRequired().HasMaxLength(500);
        builder.Property(w => w.Humidity).IsRequired();
        builder.Property(w => w.WindSpeed).IsRequired().HasPrecision(18, 2);
        builder.Property(w => w.ExpiresAt).IsRequired();
        builder.Property(w => w.CreatedAt).IsRequired();
        builder.Property(w => w.CreatedBy).IsRequired().HasMaxLength(100);

        builder.HasIndex(w => new { w.City, w.ExpiresAt });
        builder.HasIndex(w => w.ExpiresAt);
    }
}