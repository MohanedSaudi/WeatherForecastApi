using FluentValidation;

namespace WeatherApi.Application.Weather.GetWeather;


public class GetWeatherQueryValidator : AbstractValidator<GetWeatherQuery>
{
    public GetWeatherQueryValidator()
    {
        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MinimumLength(2).WithMessage("City name must be at least 2 characters")
            .MaximumLength(100).WithMessage("City name must not exceed 100 characters");
    }
}