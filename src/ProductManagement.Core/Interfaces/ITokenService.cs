using ProductManagement.Core.Entities;

namespace ProductManagement.Core.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
    DateTime GetExpirationDate();
}
