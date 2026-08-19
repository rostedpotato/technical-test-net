using ProductManagement.Core.DTOs;

namespace ProductManagement.Core.Interfaces;

public interface IProductService
{
    Task<ApiResponse<PagedResult<ProductDto>>> GetProductsAsync(ProductQueryParameters parameters);
    Task<ApiResponse<ProductDto>> GetProductByIdAsync(int id);
    Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto dto);
    Task<ApiResponse<ProductDto>> UpdateProductAsync(int id, UpdateProductDto dto);
    Task<ApiResponse<bool>> DeleteProductAsync(int id);
}
