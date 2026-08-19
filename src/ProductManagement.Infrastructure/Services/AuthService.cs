using ProductManagement.Core.DTOs;
using ProductManagement.Core.Entities;
using ProductManagement.Core.Interfaces;

namespace ProductManagement.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        var errors = new List<string>();

        if (await _userRepository.ExistsByUsernameAsync(dto.Username))
        {
            errors.Add($"Username '{dto.Username}' is already taken.");
        }

        if (await _userRepository.ExistsByEmailAsync(dto.Email))
        {
            errors.Add($"Email '{dto.Email}' is already registered.");
        }

        if (errors.Any())
        {
            return ApiResponse<AuthResponseDto>.FailureResponse("Registration failed.", errors);
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var newUser = new User
        {
            Username = dto.Username.Trim(),
            Email = dto.Email.Trim().ToLower(),
            PasswordHash = passwordHash,
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.AddAsync(newUser);
        var token = _tokenService.GenerateToken(createdUser);
        var expiresAt = _tokenService.GetExpirationDate();

        var responseData = new AuthResponseDto
        {
            Token = token,
            Username = createdUser.Username,
            Email = createdUser.Email,
            Role = createdUser.Role,
            ExpiresAt = expiresAt
        };

        return ApiResponse<AuthResponseDto>.SuccessResponse(responseData, "User registered successfully.");
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByUsernameOrEmailAsync(dto.UsernameOrEmail.Trim());

        if (user == null)
        {
            return ApiResponse<AuthResponseDto>.FailureResponse("Invalid username or password.", 
                new List<string> { "Username/Email or password is incorrect." });
        }

        var isValidPassword = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!isValidPassword)
        {
            return ApiResponse<AuthResponseDto>.FailureResponse("Invalid username or password.", 
                new List<string> { "Username/Email or password is incorrect." });
        }

        var token = _tokenService.GenerateToken(user);
        var expiresAt = _tokenService.GetExpirationDate();

        var responseData = new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            ExpiresAt = expiresAt
        };

        return ApiResponse<AuthResponseDto>.SuccessResponse(responseData, "Login successful.");
    }

    public async Task<ApiResponse<UserDto>> GetCurrentUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<UserDto>.FailureResponse("User not found.", new List<string> { "User does not exist." });
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };

        return ApiResponse<UserDto>.SuccessResponse(userDto, "User profile retrieved successfully.");
    }
}
