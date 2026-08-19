using FluentAssertions;
using Moq;
using ProductManagement.Core.DTOs;
using ProductManagement.Core.Entities;
using ProductManagement.Core.Interfaces;
using ProductManagement.Infrastructure.Services;
using Xunit;

namespace ProductManagement.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockTokenService = new Mock<ITokenService>();
        _authService = new AuthService(_mockUserRepo.Object, _mockTokenService.Object);
    }

    [Fact]
    public async Task RegisterAsync_WhenUsernameAlreadyExists_ShouldReturnFailure()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Username = "existinguser",
            Email = "test@example.com",
            Password = "Password123!"
        };

        _mockUserRepo.Setup(r => r.ExistsByUsernameAsync("existinguser")).ReturnsAsync(true);
        _mockUserRepo.Setup(r => r.ExistsByEmailAsync("test@example.com")).ReturnsAsync(false);

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Username 'existinguser' is already taken"));
        _mockUserRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ShouldReturnFailure()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Username = "newuser",
            Email = "existing@example.com",
            Password = "Password123!"
        };

        _mockUserRepo.Setup(r => r.ExistsByUsernameAsync("newuser")).ReturnsAsync(false);
        _mockUserRepo.Setup(r => r.ExistsByEmailAsync("existing@example.com")).ReturnsAsync(true);

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Email 'existing@example.com' is already registered"));
        _mockUserRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldCreateUserAndReturnToken()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Username = "validuser",
            Email = "valid@example.com",
            Password = "Password123!"
        };

        _mockUserRepo.Setup(r => r.ExistsByUsernameAsync(It.IsAny<string>())).ReturnsAsync(false);
        _mockUserRepo.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(false);
        _mockUserRepo.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) =>
            {
                u.Id = 1;
                return u;
            });
        _mockTokenService.Setup(t => t.GenerateToken(It.IsAny<User>())).Returns("mock_jwt_token");
        _mockTokenService.Setup(t => t.GetExpirationDate()).Returns(DateTime.UtcNow.AddHours(2));

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().Be("mock_jwt_token");
        result.Data.Username.Should().Be("validuser");
        _mockUserRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            UsernameOrEmail = "nonexistent",
            Password = "Password123!"
        };

        _mockUserRepo.Setup(r => r.GetByUsernameOrEmailAsync("nonexistent")).ReturnsAsync((User?)null);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("Invalid username or password.");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var rawPassword = "CorrectPassword123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);
        var user = new User
        {
            Id = 1,
            Username = "validuser",
            Email = "valid@example.com",
            PasswordHash = passwordHash,
            Role = "User"
        };

        var loginDto = new LoginDto
        {
            UsernameOrEmail = "validuser",
            Password = rawPassword
        };

        _mockUserRepo.Setup(r => r.GetByUsernameOrEmailAsync("validuser")).ReturnsAsync(user);
        _mockTokenService.Setup(t => t.GenerateToken(user)).Returns("valid_token_xyz");
        _mockTokenService.Setup(t => t.GetExpirationDate()).Returns(DateTime.UtcNow.AddHours(2));

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().Be("valid_token_xyz");
        result.Data.Username.Should().Be("validuser");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldReturnFailure()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!");
        var user = new User
        {
            Id = 1,
            Username = "validuser",
            Email = "valid@example.com",
            PasswordHash = passwordHash,
            Role = "User"
        };

        var loginDto = new LoginDto
        {
            UsernameOrEmail = "validuser",
            Password = "WrongPassword!"
        };

        _mockUserRepo.Setup(r => r.GetByUsernameOrEmailAsync("validuser")).ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
    }
}
