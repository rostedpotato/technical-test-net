using ProductManagement.Core.DTOs;
using ProductManagement.Core.Entities;
using ProductManagement.Core.Interfaces;

namespace ProductManagement.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ApiResponse<PagedResult<ProductDto>>> GetProductsAsync(ProductQueryParameters parameters)
    {
        var pagedProducts = await _productRepository.GetPagedAndFilteredAsync(parameters);

        var productDtos = pagedProducts.Items.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            CreatedAt = p.CreatedAt
        });

        var result = new PagedResult<ProductDto>
        {
            Items = productDtos,
            TotalCount = pagedProducts.TotalCount,
            Page = pagedProducts.Page,
            PageSize = pagedProducts.PageSize
        };

        return ApiResponse<PagedResult<ProductDto>>.SuccessResponse(result, "Products retrieved successfully.");
    }

    public async Task<ApiResponse<ProductDto>> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return ApiResponse<ProductDto>.FailureResponse("Product not found.", 
                new List<string> { $"Product with ID {id} does not exist." });
        }

        var dto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CreatedAt = product.CreatedAt
        };

        return ApiResponse<ProductDto>.SuccessResponse(dto, "Product retrieved successfully.");
    }

    public async Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            Price = dto.Price,
            CreatedAt = DateTime.UtcNow
        };

        var createdProduct = await _productRepository.AddAsync(product);

        var responseDto = new ProductDto
        {
            Id = createdProduct.Id,
            Name = createdProduct.Name,
            Description = createdProduct.Description,
            Price = createdProduct.Price,
            CreatedAt = createdProduct.CreatedAt
        };

        return ApiResponse<ProductDto>.SuccessResponse(responseDto, "Product created successfully.");
    }

    public async Task<ApiResponse<ProductDto>> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return ApiResponse<ProductDto>.FailureResponse("Product not found.", 
                new List<string> { $"Product with ID {id} does not exist." });
        }

        product.Name = dto.Name.Trim();
        product.Description = dto.Description.Trim();
        product.Price = dto.Price;

        await _productRepository.UpdateAsync(product);

        var responseDto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CreatedAt = product.CreatedAt
        };

        return ApiResponse<ProductDto>.SuccessResponse(responseDto, "Product updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteProductAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return ApiResponse<bool>.FailureResponse("Product not found.", 
                new List<string> { $"Product with ID {id} does not exist." });
        }

        await _productRepository.DeleteAsync(product);
        return ApiResponse<bool>.SuccessResponse(true, "Product deleted successfully.");
    }
}
