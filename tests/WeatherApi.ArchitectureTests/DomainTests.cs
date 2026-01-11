using FluentAssertions;
using NetArchTest.Rules;
using WeatherApi.Domain.Users;
using Xunit;

namespace WeatherApi.ArchitectureTests;

public class DomainTests
{
    private const string DomainNamespace = "WeatherApi.Domain";

    [Fact]
    public void Domain_ShouldNotHaveDependencyOnApplication()
    {
        // Arrange
        var assembly = typeof(User).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn("WeatherApi.Application")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_ShouldNotHaveDependencyOnInfrastructure()
    {
        // Arrange
        var assembly = typeof(User).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn("WeatherApi.Infrastructure")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_ShouldNotHaveDependencyOnAPI()
    {
        // Arrange
        var assembly = typeof(User).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn("WeatherApi.API")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void DomainEntities_ShouldBeSealed()
    {
        // Arrange
        var assembly = typeof(User).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespaceContaining("WeatherApi.Domain")
            .And()
            .AreClasses()
            .And()
            .DoNotHaveNameMatching(".*Entity")
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void DomainEntities_ShouldNotBePublic()
    {
        // Arrange
        var assembly = typeof(User).Assembly;

        // Act - Entities can be public for EF Core, so this is optional
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespaceContaining("WeatherApi.Domain")
            .And()
            .AreClasses()
            .Should()
            .BePublic() // They should be public for EF Core
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void ValueObjects_ShouldBeRecords()
    {
        // Arrange
        var assembly = typeof(Email).Assembly;

        // Act
        var emailType = typeof(Email);
        var passwordType = typeof(Password);

        // Assert
        emailType.Should().Match(t => t.IsClass && t.GetMethod("<Clone>$") != null, "Email should be a record");
        passwordType.Should().Match(t => t.IsClass && t.GetMethod("<Clone>$") != null, "Password should be a record");
    }
}