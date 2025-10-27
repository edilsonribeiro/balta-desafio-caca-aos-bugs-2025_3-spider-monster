using BugStore.Data;
using BugStore.Models;
using Microsoft.EntityFrameworkCore;
using CreateProductRequest = BugStore.Requests.Products.Create;
using UpdateProductRequest = BugStore.Requests.Products.Update;
using CreateProductResponse = BugStore.Responses.Products.Create;
using GetProductByIdResponse = BugStore.Responses.Products.GetById;
using GetProductsResponse = BugStore.Responses.Products.Get;
using UpdateProductResponse = BugStore.Responses.Products.Update;

namespace BugStore.Handlers.Products;

public class Handler(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<IReadOnlyList<GetProductsResponse>> GetAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .OrderBy(product => product.Title)
            .Select(product => new GetProductsResponse(
                product.Id,
                product.Title,
                product.Description,
                product.Slug,
                product.Price))
            .ToListAsync(cancellationToken);
    }

    public async Task<GetProductByIdResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        return product is null
            ? null
            : new GetProductByIdResponse(
                product.Id,
                product.Title,
                product.Description,
                product.Slug,
                product.Price);
    }

    public async Task<CreateProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Slug = request.Slug,
            Price = request.Price
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateProductResponse(
            product.Id,
            product.Title,
            product.Description,
            product.Slug,
            product.Price);
    }

    public async Task<UpdateProductResponse?> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (product is null)
            return null;

        product.Title = request.Title;
        product.Description = request.Description;
        product.Slug = request.Slug;
        product.Price = request.Price;

        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateProductResponse(
            product.Id,
            product.Title,
            product.Description,
            product.Slug,
            product.Price);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (product is null)
            return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
