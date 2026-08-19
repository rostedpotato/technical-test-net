using FluentAssertions;
using Moq;
using ProductManagement.Core.DTOs;
using ProductManagement.Core.Entities;
using ProductManagement.Core.Interfaces;
using ProductManagement.Infrastructure.Services;
using Xunit;

namespace ProductManagement.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockRepo;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _mockRepo = new Mock<IProductRepository>();
        _service = new ProductService(_mockRepo.Object);
    }

    [Fact]
    public async Task GetProductsAsync_ShouldReturnPagedProducts()
    {
        // Arrange
        var parameters = new ProductQueryParameters { Keyword = "Laptop", MinPrice = 500, MaxPrice = 2000, Page = 1, PageSize = 10 };
        var sampleProducts = new List<Product>
        {
            new Product { Id = 1, Name = "Laptop Pro", Description = "Test laptop", Price = 1200, CreatedAt = DateTime.UtcNow }
        };

        var pagedResult = new PagedResult<Product>
        {
            Items = sampleProducts,
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };

        _mockRepo.Setup(r => r.GetPagedAndFilteredAsync(parameters))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.GetProductsAsync(parameters);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalCount.Should().Be(1);
        result.Data.Items.Should().HaveCount(1);
        result.Data.Items.First().Name.Should().Be("Laptop Pro");
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductExists_ShouldReturnProduct()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Mouse", Description = "Gaming mouse", Price = 50, CreatedAt = DateTime.UtcNow };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        // Act
        var result = await _service.GetProductByIdAsync(1);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(1);
        result.Data.Name.Should().Be("Mouse");
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        // Act
        var result = await _service.GetProductByIdAsync(999);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateProductAsync_WithValidDto_ShouldCreateAndReturnProduct()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "Monitor 4K",
            Description = "32-inch 4K IPS display",
            Price = 450.00m
        };

        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) =>
            {
                p.Id = 10;
                return p;
            });

        // Act
        var result = await _service.CreateProductAsync(createDto);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(10);
        result.Data.Name.Should().Be("Monitor 4K");
        result.Data.Price.Should().Be(450.00m);
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_WhenProductExists_ShouldUpdateAndReturnProduct()
    {
        // Arrange
        var existingProduct = new Product { Id = 5, Name = "Old Name", Description = "Old Desc", Price = 100, CreatedAt = DateTime.UtcNow };
        var updateDto = new UpdateProductDto { Name = "New Name", Description = "New Desc", Price = 150 };

        _mockRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(existingProduct);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateProductAsync(5, updateDto);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("New Name");
        result.Data.Price.Should().Be(150);
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_WhenProductDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        var updateDto = new UpdateProductDto { Name = "New Name", Description = "New Desc", Price = 150 };
        _mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        // Act
        var result = await _service.UpdateProductAsync(999, updateDto);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProductAsync_WhenProductExists_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        var product = new Product { Id = 3, Name = "Keyboard", Description = "Mechanical", Price = 80, CreatedAt = DateTime.UtcNow };
        _mockRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(product);
        _mockRepo.Setup(r => r.DeleteAsync(product)).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteProductAsync(3);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();
        _mockRepo.Verify(r => r.DeleteAsync(product), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_WhenProductDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        // Act
        var result = await _service.DeleteProductAsync(999);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        _mockRepo.Verify(r => r.DeleteAsync(It.IsAny<Product>()), Times.Never);
    }
}
