namespace WeatherApi.Infrastructure.Weather;

public class WeatherApiSettings
{
    public const string SectionName = "WeatherApi";

    public string BaseUrl { get; set; } = "https://api.openweathermap.org/data/2.5/";
    public string ApiKey { get; set; } = string.Empty;
    public bool UseFakeData { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 10;
    public int RetryCount { get; set; } = 3;
}