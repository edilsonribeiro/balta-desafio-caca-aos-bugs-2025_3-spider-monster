using BugStore.Handlers.Products;
using BugStore.Models;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Test;

public class ProductsHandlerTests
{
    [Fact]
    public async Task GetAsync_ReturnsProductsOrderedByTitle()
    {
        using var context = TestDbContextFactory.CreateContext();
        var ct = TestContext.Current.CancellationToken;
        context.Products.AddRange(
            new Product
            {
                Id = Guid.NewGuid(),
                Title = "Zebra Plush",
                Description = "Soft zebra plush toy",
                Slug = "zebra-plush",
                Price = 25m
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Title = "Alien Figure",
                Description = "Limited edition alien action figure",
                Slug = "alien-figure",
                Price = 50m
            });
        await context.SaveChangesAsync(ct);
        var handler = new Handler(context);

        var products = await handler.GetAsync(ct);

        Assert.Collection(products,
            first => Assert.Equal("Alien Figure", first.Title),
            second => Assert.Equal("Zebra Plush", second.Title));
    }

    [Fact]
    public async Task CreateAsync_PersistsProduct()
    {
        using var context = TestDbContextFactory.CreateContext();
        var ct = TestContext.Current.CancellationToken;
        var handler = new Handler(context);
        var request = new BugStore.Requests.Products.Create
        {
            Title = "Spider Drone",
            Description = "Autonomous surveillance drone",
            Slug = "spider-drone",
            Price = 199.99m
        };

        var response = await handler.CreateAsync(request, ct);

        Assert.Equal(request.Title, response.Title);
        Assert.NotEqual(Guid.Empty, response.Id);
        var saved = await context.Products.SingleAsync(p => p.Id == response.Id, ct);
        Assert.Equal(request.Slug, saved.Slug);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNullWhenProductDoesNotExist()
    {
        using var context = TestDbContextFactory.CreateContext();
        var ct = TestContext.Current.CancellationToken;
        var handler = new Handler(context);
        var request = new BugStore.Requests.Products.Update
        {
            Title = "Updated Product",
            Description = "New description",
            Slug = "updated-product",
            Price = 99m
        };

        var response = await handler.UpdateAsync(Guid.NewGuid(), request, ct);

        Assert.Null(response);
    }

    [Fact]
    public async Task DeleteAsync_RemovesProduct()
    {
        using var context = TestDbContextFactory.CreateContext();
        var ct = TestContext.Current.CancellationToken;
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Title = "Phantom Camera",
            Description = "High-speed camera",
            Slug = "phantom-camera",
            Price = 1299m
        };
        context.Products.Add(product);
        await context.SaveChangesAsync(ct);
        var handler = new Handler(context);

        var result = await handler.DeleteAsync(product.Id, ct);

        Assert.True(result);
        Assert.False(await context.Products.AnyAsync(p => p.Id == product.Id, ct));
    }
}
