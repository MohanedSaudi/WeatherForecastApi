using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NetArchTest.Rules;
using WeatherApi.API.Controllers;
using WeatherApi.Application.Users.Register;
using WeatherApi.Domain.Users;
using WeatherApi.Infrastructure.Users;

namespace WeatherApi.ArchitectureTests;

public class ArchitectureTests
{
    private const string DomainNamespace = "WeatherApi.Domain";
    private const string ApplicationNamespace = "WeatherApi.Application";
    private const string InfrastructureNamespace = "WeatherApi.Infrastructure";
    private const string ApiNamespace = "WeatherApi.API";

    [Fact]
    public void Domain_Should_NotHaveDependencyOnOtherProjects()
    {
        // Arrange
        var assembly = typeof(User).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAll(ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_Should_NotHaveDependencyOnInfrastructureAndApi()
    {
        // Arrange
        var assembly = typeof(RegisterCommand).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAll(InfrastructureNamespace, ApiNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Handlers_Should_HaveNameEndingWithHandler()
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
    public void Controllers_Should_HaveNameEndingWithController()
    {
        // Arrange
        var assembly = typeof(AuthController).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .Inherit(typeof(ControllerBase))
            .Should()
            .HaveNameEndingWith("Controller")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }


    [Fact]
    public void Infrastructure_Should_NotHaveDependencyOnApi()
    {
        // Arrange
        var assembly = typeof(UserRepository).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }


    [Fact]
    public void Validators_Should_HaveNameEndingWithValidator()
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
    public void Repositories_Should_HaveNameEndingWithRepository()
    {
        // Arrange
        var assembly = typeof(UserRepository).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespaceEndingWith("Repositories")
            .Or()
            .HaveNameEndingWith("Repository")
            .Should()
            .HaveNameEndingWith("Repository")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_Entities_Should_BeSealed()
    {
        // Arrange
        var assembly = typeof(User).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace(DomainNamespace)
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
}
