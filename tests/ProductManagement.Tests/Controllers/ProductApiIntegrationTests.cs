using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using ProductManagement.Core.DTOs;
using Xunit;

namespace ProductManagement.Tests.Controllers;

public class ProductApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProductApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_Endpoint_ReturnsSuccessAndProductList()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ProductDto>>>();
        content.Should().NotBeNull();
        content!.Success.Should().BeTrue();
        content.Data.Should().NotBeNull();
        content.Data!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateProduct_WithoutAuthToken_ReturnsUnauthorized()
    {
        // Arrange
        var newProduct = new CreateProductDto
        {
            Name = "Unauthorized Tablet",
            Description = "Should not be created",
            Price = 299.99m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", newProduct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AuthLogin_And_CreateProduct_WithToken_Succeeds()
    {
        // 1. Authenticate
        var loginDto = new LoginDto
        {
            UsernameOrEmail = "admin",
            Password = "Admin123!"
        };

        var loginRes = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        loginRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginData = await loginRes.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
        loginData.Should().NotBeNull();
        loginData!.Data.Should().NotBeNull();
        var token = loginData.Data!.Token;

        // 2. Create Product with Auth Header
        var newProduct = new CreateProductDto
        {
            Name = "Integration Test Gaming Mouse",
            Description = "16000 DPI sensor with RGB lighting",
            Price = 69.99m
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/products")
        {
            Content = JsonContent.Create(newProduct)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRes = await _client.SendAsync(request);
        createRes.StatusCode.Should().Be(HttpStatusCode.Created);

        var createData = await createRes.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        createData.Should().NotBeNull();
        createData!.Success.Should().BeTrue();
        createData.Data!.Name.Should().Be("Integration Test Gaming Mouse");
    }
}
