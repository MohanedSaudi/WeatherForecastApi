namespace WeatherApi.Application.Weather.DTOs;

public record WeatherDto(
    string City,
    DateTime Date,
    int TemperatureC,
    int TemperatureF,
    string Summary,
    string Description,
    int Humidity,
    double WindSpeed);