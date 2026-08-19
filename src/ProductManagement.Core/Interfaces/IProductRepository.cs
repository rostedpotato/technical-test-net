using ProductManagement.Core.DTOs;
using ProductManagement.Core.Entities;

namespace ProductManagement.Core.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<PagedResult<Product>> GetPagedAndFilteredAsync(ProductQueryParameters parameters);
    Task<Product> AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);
    Task<bool> ExistsAsync(int id);
}
