//using FluentAssertions;
//using FluentValidation;
//using MediatR;
//using NetArchTest.Rules;
//using WeatherApi.Application.Users.Register;
//using WeatherApi.Domain.Users;
//using Xunit;

//namespace WeatherApi.ArchitectureTests;

//public class ApplicationTests
//{
//    private const string ApplicationNamespace = "WeatherApi.Application";

//    [Fact]
//    public void Application_ShouldNotHaveDependencyOnInfrastructure()
//    {
//        // Arrange
//        var assembly = typeof(RegisterCommand).Assembly;

//        // Act
//        var result = Types.InAssembly(assembly)
//            .ShouldNot()
//            .HaveDependencyOn("WeatherApi.Infrastructure")
//            .GetResult();

//        // Assert
//        result.IsSuccessful.Should().BeTrue();
//    }

//    [Fact]
//    public void Application_ShouldNotHaveDependencyOnAPI()
//    {
//        // Arrange
//        var assembly = typeof(RegisterCommand).Assembly;

//        // Act
//        var result = Types.InAssembly(assembly)
//            .ShouldNot()
//            .HaveDependencyOn("WeatherApi.API")
//            .GetResult();

//        // Assert
//        result.IsSuccessful.Should().BeTrue();
//    }

//    [Fact]
//    public void CommandHandlers_ShouldHaveNameEndingWithHandler()
//    {
//        // Arrange
//        var assembly = typeof(RegisterCommandHandler).Assembly;

//        // Act
//        var result = Types.InAssembly(assembly)
//            .That()
//            .ImplementInterface(typeof(IRequestHandler<,>))
//            .Should()
//            .HaveNameEndingWith("Handler")
//            .GetResult();

//        // Assert
//        result.IsSuccessful.Should().BeTrue();
//    }

//    [Fact]
//    public void Commands_ShouldBeImmutable()
//    {
//        // Arrange
//        var assembly = typeof(RegisterCommand).Assembly;

//        // Act - Commands should be records (immutable)
//        var commandTypes = Types.InAssembly(assembly)
//            .That()
//            .HaveNameEndingWith("Command")
//            .GetTypes();

//        // Assert
//        foreach (var commandType in commandTypes)
//        {
//            commandType.Should().Match(t => t.IsClass && t.GetMethod("<Clone>$") != null,
//                $"{commandType.Name} should be a record (immutable)");
//        }
//    }

//    [Fact]
//    public void Validators_ShouldHaveNameEndingWithValidator()
//    {
//        // Arrange
//        var assembly = typeof(RegisterCommand).Assembly;

//        // Act
//        var result = Types.InAssembly(assembly)
//            .That()
//            .Inherit(typeof(AbstractValidator<>))
//            .Should()
//            .HaveNameEndingWith("Validator")
//            .GetResult();

//        // Assert
//        result.IsSuccessful.Should().BeTrue();
//    }

//    [Fact]
//    public void Queries_ShouldHaveNameEndingWithQuery()
//    {
//        // Arrange
//        var assembly = typeof(RegisterCommand).Assembly;

//        // Act
//        var result = Types.InAssembly(assembly)
//            .That()
//            .ResideInNamespaceContaining("Queries")
//            .Or()
//            .HaveNameEndingWith("Query")
//            .Should()
//            .HaveNameEndingWith("Query")
//            .GetResult();

//        // Assert
//        result.IsSuccessful.Should().BeTrue();
//    }

//    [Fact]
//    public void DTOs_ShouldBeRecords()
//    {
//        // Arrange
//        var assembly = typeof(RegisterCommand).Assembly;

//        // Act
//        var dtoTypes = Types.InAssembly(assembly)
//            .That()
//            .ResideInNamespaceEndingWith("DTOs")
//            .GetTypes();

//        // Assert
//        foreach (var dtoType in dtoTypes)
//        {
//            [InlineData("pass123!")] // No uppercase
//            [InlineData("PASS123!")] // No lowercase
//            [InlineData("Password!")] // No digit
//            [InlineData("Pass1!")] // Too short
//            [InlineData("short")] // Too short, missing requirements
//            public void Create_WithInvalidPassword_ShouldFail(string password)
//            {
//                // Act
//                var result = Password.Create(password);

//                // Assert
//                result.IsFailure.Should().BeTrue();
//                result.Error.Should().Be(UserErrors.WeakPassword);
//            }

//            [Fact]
//            public void Create_WithNullPassword_ShouldFail()
//            {
//                // Act
//                var result = Password.Create(null!);

//                // Assert
//                result.IsFailure.Should().BeTrue();
//            }

//            [Fact]
//            public void ImplicitConversion_ToString_ShouldWork()
//            {
//                // Arrange
//                var passwordResult = Password.Create("Pass123!@#");
//                string passwordString = passwordResult.Value;

//                // Assert
//                passwordString.Should().Be("Pass123!@#");
//            }
//        }