using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using WeatherApi.Application.Common.Interfaces;
using WeatherApi.Application.Users.Login;
using WeatherApi.Domain.Common.Interfaces;
using WeatherApi.Domain.Enums;
using WeatherApi.Domain.RefreshTokens;
using WeatherApi.Domain.Users;
using Xunit;

namespace WeatherApi.UnitTests.Application.Users;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<ILogger<LoginCommandHandler>> _loggerMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _jwtServiceMock = new Mock<IJwtService>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _loggerMock = new Mock<ILogger<LoginCommandHandler>>();

        _handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _jwtServiceMock.Object,
            _httpContextAccessorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldSucceed()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "Test123!@#");

        var emailResult = Email.Create("test@example.com");
        var passwordResult = Password.Create("Test123!@#");
        var user = User.Create("testuser", emailResult.Value, passwordResult.Value).Value;

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtServiceMock
            .Setup(x => x.GenerateTokens(It.IsAny<User>()))
            .Returns(("access_token", "refresh_token"));

        var httpContext = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("test@example.com");
        result.Value.AccessToken.Should().Be("access_token");
    }

    [Fact]
    public async Task Handle_WithNonExistentEmail_ShouldFail()
    {
        // Arrange
        var command = new LoginCommand("nonexistent@example.com", "Test123!@#");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ShouldFail()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "WrongPassword");

        var emailResult = Email.Create("test@example.com");
        var passwordResult = Password.Create("Test123!@#");
        var user = User.Create("testuser", emailResult.Value, passwordResult.Value).Value;

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldFail()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "Test123!@#");

        var emailResult = Email.Create("test@example.com");
        var passwordResult = Password.Create("Test123!@#");
        var user = User.Create("testuser", emailResult.Value, passwordResult.Value).Value;
        user.Deactivate();

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.Inactive);
    }
}