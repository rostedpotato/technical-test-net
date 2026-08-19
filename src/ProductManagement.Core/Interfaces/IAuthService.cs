using ProductManagement.Core.DTOs;

namespace ProductManagement.Core.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto);
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto);
    Task<ApiResponse<UserDto>> GetCurrentUserAsync(int userId);
}
