using FluentAssertions;
using FluentValidation;
using MediatR;
using NetArchTest.Rules;
using WeatherApi.Application.Users.Register;
using Xunit;

namespace WeatherApi.ArchitectureTests;

public class ApplicationTests
{
    private const string ApplicationNamespace = "WeatherApi.Application";

    [Fact]
    public void Application_ShouldNotHaveDependencyOnInfrastructure()
    {
        // Arrange
        var assembly = typeof(RegisterCommand).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn("WeatherApi.Infrastructure")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_ShouldNotHaveDependencyOnAPI()
    {
        // Arrange
        var assembly = typeof(RegisterCommand).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn("WeatherApi.API")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void CommandHandlers_ShouldHaveNameEndingWithHandler()
    {
        // Arrange
        var assembly = typeof(RegisterCommandHandler).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ImplementInterface(typeof(IRequestHandler<,>))
            .Should()
            .HaveNameEndingWith("Handler")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Commands_ShouldBeImmutable()
    {
        // Arrange
        var assembly = typeof(RegisterCommand).Assembly;

        // Act - Commands should be records (immutable)
        var commandTypes = Types.InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Command")
            .GetTypes();

        // Assert
        foreach (var commandType in commandTypes)
        {
            commandType.Should().Match(t => t.IsClass && t.GetMethod("<Clone>$") != null,
                $"{commandType.Name} should be a record (immutable)");
        }
    }

    [Fact]
    public void Validators_ShouldHaveNameEndingWithValidator()
    {
        // Arrange
        var assembly = typeof(RegisterCommand).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .Inherit(typeof(AbstractValidator<>))
            .Should()
            .HaveNameEndingWith("Validator")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Queries_ShouldHaveNameEndingWithQuery()
    {
        // Arrange
        var assembly = typeof(RegisterCommand).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespaceContaining("Queries")
            .Or()
            .HaveNameEndingWith("Query")
            .Should()
            .HaveNameEndingWith("Query")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void DTOs_ShouldBeRecords()
    {
        // Arrange
        var assembly = typeof(RegisterCommand).Assembly;

        // Act
        var dtoTypes = Types.InAssembly(assembly)
            .That()
            .ResideInNamespaceEndingWith("DTOs")
            .GetTypes();

        // Assert
        foreach (var dtoType in dtoTypes)
        {
            dtoType.Should().Match(t => t.IsClass && t.GetMethod("<Clone>$") != null,
                $"{dtoType.Name} should be a record");
        }
    }
}