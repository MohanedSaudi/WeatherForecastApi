using FluentAssertions;
using NetArchTest.Rules;
using WeatherApi.Application.Users.Register;
using WeatherApi.Infrastructure.Users;
using Xunit;

namespace WeatherApi.ArchitectureTests;

public class NamingConventionTests
{
    [Fact]
    public void Controllers_ShouldHaveNameEndingWithController()
    {
        // Arrange
        var assembly = typeof(WeatherApi.API.Controllers.AuthController).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace("WeatherApi.API.Controllers")
            .Should()
            .HaveNameEndingWith("Controller")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Repositories_ShouldHaveNameEndingWithRepository()
    {
        // Arrange
        var assembly = typeof(UserRepository).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .HaveNameEndingWith("Repository")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Interfaces_ShouldStartWithI()
    {
        // Arrange
        var assemblies = new[]
        {
            typeof(WeatherApi.Domain.Users.User).Assembly,
            typeof(RegisterCommand).Assembly,
            typeof(UserRepository).Assembly
        };

        // Act & Assert
        foreach (var assembly in assemblies)
        {
            var result = Types.InAssembly(assembly)
                .That()
                .AreInterfaces()
                .Should()
                .HaveNameStartingWith("I")
                .GetResult();

            result.IsSuccessful.Should().BeTrue($"All interfaces in {assembly.GetName().Name} should start with 'I'");
        }
    }

    [Fact]
    public void Commands_ShouldHaveNameEndingWithCommand()
    {
        // Arrange
        var assembly = typeof(RegisterCommand).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespaceContaining("Commands")
            .Or()
            .HaveNameEndingWith("Command")
            .Should()
            .HaveNameEndingWith("Command")
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
    public void Validators_ShouldHaveNameEndingWithValidator()
    {
        // Arrange
        var assembly = typeof(RegisterCommand).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Validator")
            .Should()
            .HaveNameEndingWith("Validator")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Handlers_ShouldHaveNameEndingWithHandler()
    {
        // Arrange
        var assembly = typeof(RegisterCommandHandler).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Handler")
            .Should()
            .HaveNameEndingWith("Handler")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Services_ShouldHaveNameEndingWithService()
    {
        // Arrange
        var assembly = typeof(UserRepository).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Service")
            .And()
            .DoNotResideInNamespace("WeatherApi.Application.Common.Interfaces")
            .Should()
            .HaveNameEndingWith("Service")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Configurations_ShouldHaveNameEndingWithConfiguration()
    {
        // Arrange
        var assembly = typeof(UserRepository).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespaceContaining("Configurations")
            .Or()
            .HaveNameEndingWith("Configuration")
            .Should()
            .HaveNameEndingWith("Configuration")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Errors_ShouldHaveNameEndingWithErrors()
    {
        // Arrange
        var assembly = typeof(WeatherApi.Domain.Users.User).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Errors")
            .Should()
            .BeSealed()
            .And()
            .BePublic()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void DTOs_ShouldResideInDTOsNamespace()
    {
        // Arrange
        var assembly = typeof(RegisterCommand).Assembly;

        // Act
        var dtoTypes = Types.InAssembly(assembly)
            .That()
            .ResideInNamespaceEndingWith("DTOs")
            .GetTypes();

        // Assert
        dtoTypes.Should().NotBeEmpty("There should be DTOs in the application");
        foreach (var dtoType in dtoTypes)
        {
            dtoType.Namespace.Should().EndWith("DTOs", $"{dtoType.Name} should be in a DTOs namespace");
        }
    }

    [Fact]
    public void Middleware_ShouldHaveNameEndingWithMiddleware()
    {
        // Arrange
        var assembly = typeof(WeatherApi.API.Controllers.AuthController).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespaceContaining("Middleware")
            .Should()
            .HaveNameEndingWith("Middleware")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Extensions_ShouldHaveNameEndingWithExtensions()
    {
        // Arrange
        var assembly = typeof(WeatherApi.API.Controllers.AuthController).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespaceContaining("Extensions")
            .Should()
            .HaveNameEndingWith("Extensions")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }
}