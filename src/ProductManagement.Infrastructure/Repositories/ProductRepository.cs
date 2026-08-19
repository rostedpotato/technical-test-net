using Microsoft.EntityFrameworkCore;
using ProductManagement.Core.DTOs;
using ProductManagement.Core.Entities;
using ProductManagement.Core.Interfaces;
using ProductManagement.Infrastructure.Data;

namespace ProductManagement.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<PagedResult<Product>> GetPagedAndFilteredAsync(ProductQueryParameters parameters)
    {
        var query = _context.Products.AsNoTracking().AsQueryable();

        // Filter by keyword (Name or Description)
        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(keyword) || p.Description.ToLower().Contains(keyword));
        }

        // Filter by MinPrice
        if (parameters.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= parameters.MinPrice.Value);
        }

        // Filter by MaxPrice
        if (parameters.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= parameters.MaxPrice.Value);
        }

        var totalCount = await query.CountAsync();

        // Dynamic Sorting (Casting decimal to double for SQLite ORDER BY support)
        query = (parameters.SortBy?.ToLower(), parameters.SortDescending) switch
        {
            ("name", true) => query.OrderByDescending(p => p.Name),
            ("name", false) => query.OrderBy(p => p.Name),
            ("price", true) => query.OrderByDescending(p => (double)p.Price),
            ("price", false) => query.OrderBy(p => (double)p.Price),
            ("createdat", false) => query.OrderBy(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt) // default
        };

        var page = parameters.Page > 0 ? parameters.Page : 1;
        var pageSize = parameters.PageSize > 0 ? parameters.PageSize : 10;

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Product>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Product> AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Product product)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Products.AnyAsync(p => p.Id == id);
    }
}
