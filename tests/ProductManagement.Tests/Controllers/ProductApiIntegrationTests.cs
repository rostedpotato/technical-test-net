using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using ProductManagement.Core.DTOs;
using Xunit;

namespace ProductManagement.Tests.Controllers;

public class ProductApiIntegrationTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;
    private readonly HttpClient _client;

    public ProductApiIntegrationTests(TestApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_Endpoint_ReturnsSuccessAndProductList()
    {
        var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/products");

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

    [Fact]
    public async Task ProductCrud_WithAdminToken_Succeeds()
    {
        var client = await CreateAuthenticatedClientAsync();
        var productName = $"CRUD Product {Guid.NewGuid():N}";

        var createResponse = await client.PostAsJsonAsync("/api/products", new CreateProductDto
        {
            Name = productName,
            Description = "Created for the authenticated CRUD test",
            Price = 125.50m
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        created!.Data.Should().NotBeNull();

        var id = created.Data!.Id;
        var updateResponse = await client.PutAsJsonAsync($"/api/products/{id}", new UpdateProductDto
        {
            Name = $"{productName} Updated",
            Description = "Updated description",
            Price = 150.75m
        });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await client.GetAsync($"/api/products/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrieved = await getResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        retrieved!.Data!.Price.Should().Be(150.75m);

        var deleteResponse = await client.DeleteAsync($"/api/products/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var missingResponse = await client.GetAsync($"/api/products/{id}");
        missingResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SearchAndPriceRange_ReturnsMatchingProducts()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/products?keyword=Laptop&minPrice=1000&maxPrice=2000");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ProductDto>>>();
        result!.Data.Should().NotBeNull();
        result.Data!.Items.Should().Contain(product => product.Name.Contains("Laptop"));
        result.Data.Items.Should().OnlyContain(product => product.Price >= 1000m && product.Price <= 2000m);
    }

    [Fact]
    public async Task InvalidPriceRange_ReturnsBadRequest()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/products?minPrice=500&maxPrice=100");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ProductDto>>>();
        result!.Success.Should().BeFalse();
        result.Errors.Should().Contain("MinPrice cannot be greater than MaxPrice.");
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            UsernameOrEmail = "admin",
            Password = "Admin123!"
        });
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginData = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
        loginData!.Data.Should().NotBeNull();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginData.Data!.Token);
        return client;
    }
}

public sealed class TestApplicationFactory : WebApplicationFactory<Program>
{
    public TestApplicationFactory()
    {
        Environment.SetEnvironmentVariable(
            "JwtSettings__Secret",
            "integration-test-secret-that-is-at-least-32-bytes");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }
}
