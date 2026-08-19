using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductManagement.Core.Entities;

namespace ProductManagement.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context, ILogger logger)
    {
        try
        {
            await context.Database.MigrateAsync();

            // Seed Users if empty
            if (!await context.Users.AnyAsync())
            {
                logger.LogInformation("Seeding initial users...");

                var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");
                var userPasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!");

                var users = new List<User>
                {
                    new User
                    {
                        Username = "admin",
                        Email = "admin@example.com",
                        PasswordHash = adminPasswordHash,
                        Role = "Admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Username = "demo_user",
                        Email = "user@example.com",
                        PasswordHash = userPasswordHash,
                        Role = "User",
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await context.Users.AddRangeAsync(users);
                await context.SaveChangesAsync();
            }

            // Seed Products if empty
            if (!await context.Products.AnyAsync())
            {
                logger.LogInformation("Seeding initial products...");

                var products = new List<Product>
                {
                    new Product
                    {
                        Name = "Laptop Pro 15",
                        Description = "High performance laptop with 16GB RAM and 512GB SSD for professional software engineers.",
                        Price = 1499.99m,
                        CreatedAt = DateTime.UtcNow.AddDays(-10)
                    },
                    new Product
                    {
                        Name = "Wireless Noise-Cancelling Headphones",
                        Description = "Over-ear bluetooth headphones with active noise cancellation and 30-hour battery life.",
                        Price = 249.50m,
                        CreatedAt = DateTime.UtcNow.AddDays(-8)
                    },
                    new Product
                    {
                        Name = "Mechanical Keyboard RGB",
                        Description = "Compact 75% mechanical gaming keyboard with hot-swappable switches and custom RGB lighting.",
                        Price = 89.99m,
                        CreatedAt = DateTime.UtcNow.AddDays(-6)
                    },
                    new Product
                    {
                        Name = "Ergonomic Office Chair",
                        Description = "Breathable mesh chair with adjustable lumbar support and 3D armrests.",
                        Price = 320.00m,
                        CreatedAt = DateTime.UtcNow.AddDays(-4)
                    },
                    new Product
                    {
                        Name = "UltraWide Monitor 34-inch",
                        Description = "WQHD curved IPS display with 144Hz refresh rate and USB-C power delivery.",
                        Price = 599.00m,
                        CreatedAt = DateTime.UtcNow.AddDays(-2)
                    },
                    new Product
                    {
                        Name = "Precision Wireless Mouse",
                        Description = "Ergonomic wireless mouse with customizable DPI and silent click buttons.",
                        Price = 45.00m,
                        CreatedAt = DateTime.UtcNow.AddDays(-1)
                    }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }

            logger.LogInformation("Database initialization completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }
}
