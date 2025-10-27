using BugStore.Handlers.Orders;
using BugStore.Models;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Test;

public class OrdersHandlerTests
{
    [Fact]
    public async Task CreateAsync_ReturnsNullWhenLinesAreInvalid()
    {
        using var context = TestDbContextFactory.CreateContext();
        var ct = TestContext.Current.CancellationToken;
        var handler = new Handler(context);
        var request = new BugStore.Requests.Orders.Create
        {
            CustomerId = Guid.NewGuid(),
            Lines =
            [
                new BugStore.Requests.Orders.Create.Line(Guid.NewGuid(), 0)
            ]
        };

        var response = await handler.CreateAsync(request, ct);

        Assert.Null(response);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNullWhenCustomerDoesNotExist()
    {
        using var context = TestDbContextFactory.CreateContext();
        var ct = TestContext.Current.CancellationToken;
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Title = "Night Vision Goggles",
            Description = "Infrared goggles",
            Slug = "night-vision-goggles",
            Price = 299m
        };
        context.Products.Add(product);
        await context.SaveChangesAsync(ct);
        var handler = new Handler(context);
        var request = new BugStore.Requests.Orders.Create
        {
            CustomerId = Guid.NewGuid(),
            Lines =
            [
                new(product.Id, 1)
            ]
        };

        var response = await handler.CreateAsync(request, ct);

        Assert.Null(response);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNullWhenProductDoesNotExist()
    {
        using var context = TestDbContextFactory.CreateContext();
        var ct = TestContext.Current.CancellationToken;
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = "Gwen Stacy",
            Email = "gwen@empireu.edu",
            Phone = "8888-8888",
            BirthDate = new DateTime(1997, 1, 5)
        };
        context.Customers.Add(customer);
        await context.SaveChangesAsync(ct);
        var handler = new Handler(context);
        var missingProductId = Guid.NewGuid();
        var request = new BugStore.Requests.Orders.Create
        {
            CustomerId = customer.Id,
            Lines =
            [
                new(missingProductId, 1)
            ]
        };

        var response = await handler.CreateAsync(request, ct);

        Assert.Null(response);
    }

    [Fact]
    public async Task CreateAsync_PersistsOrderWhenDataIsValid()
    {
        using var context = TestDbContextFactory.CreateContext();
        var ct = TestContext.Current.CancellationToken;
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = "Peter Parker",
            Email = "peter.parker@dailybugle.net",
            Phone = "6666-6666",
            BirthDate = new DateTime(1995, 8, 10)
        };
        var productOne = new Product
        {
            Id = Guid.NewGuid(),
            Title = "Web Shooter",
            Description = "Wrist-mounted web shooter",
            Slug = "web-shooter",
            Price = 150m
        };
        var productTwo = new Product
        {
            Id = Guid.NewGuid(),
            Title = "Spider Tracer",
            Description = "Tracking device",
            Slug = "spider-tracer",
            Price = 20m
        };
        context.Customers.Add(customer);
        context.Products.AddRange(productOne, productTwo);
        await context.SaveChangesAsync(ct);
        var handler = new Handler(context);
        var request = new BugStore.Requests.Orders.Create
        {
            CustomerId = customer.Id,
            Lines =
            [
                new(productOne.Id, 2),
                new(productTwo.Id, 3)
            ]
        };

        var response = await handler.CreateAsync(request, ct);

        Assert.NotNull(response);
        Assert.Equal(customer.Id, response!.CustomerId);
        Assert.Equal(2, response.Lines.Count);
        var expectedTotal = (productOne.Price * 2) + (productTwo.Price * 3);
        Assert.Equal(expectedTotal, response.Total);

        var savedOrder = await context.Orders.Include(o => o.Lines).SingleAsync(o => o.Id == response.Id, ct);
        Assert.Equal(2, savedOrder.Lines.Count);
        Assert.All(savedOrder.Lines, line => Assert.True(line.Total > 0));
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullWhenOrderDoesNotExist()
    {
        using var context = TestDbContextFactory.CreateContext();
        var ct = TestContext.Current.CancellationToken;
        var handler = new Handler(context);

        var response = await handler.GetByIdAsync(Guid.NewGuid(), ct);

        Assert.Null(response);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsOrderWithLines()
    {
        using var context = TestDbContextFactory.CreateContext();
        var ct = TestContext.Current.CancellationToken;
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = "Mary Jane Watson",
            Email = "mj@dailybugle.net",
            Phone = "7777-7777",
            BirthDate = new DateTime(1996, 4, 17)
        };
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Title = "Spider Drone",
            Description = "Remote drone",
            Slug = "spider-drone",
            Price = 199.99m
        };
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            CustomerId = customer.Id,
            Customer = customer,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        order.Lines.Add(new OrderLine
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = product.Id,
            Product = product,
            Quantity = 4,
            Total = product.Price * 4
        });
        context.Customers.Add(customer);
        context.Products.Add(product);
        context.Orders.Add(order);
        await context.SaveChangesAsync(ct);
        var handler = new Handler(context);

        var response = await handler.GetByIdAsync(orderId, ct);

        Assert.NotNull(response);
        Assert.Equal(orderId, response!.Id);
        Assert.Single(response.Lines);
        var line = response.Lines[0];
        Assert.Equal(product.Id, line.ProductId);
        Assert.Equal(product.Title, line.ProductTitle);
        Assert.Equal(product.Price * 4, line.Total);
        Assert.Equal(product.Price * 4, response.Total);
    }
}
