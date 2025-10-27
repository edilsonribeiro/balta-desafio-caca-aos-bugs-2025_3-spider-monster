using BugStore.Handlers.Customers;
using BugStore.Models;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Test;

public class CustomersHandlerTests
{
    [Fact]
    public async Task GetAsync_ReturnsCustomersOrderedByName()
    {
        using var context = TestDbContextFactory.CreateContext();
        var ct = TestContext.Current.CancellationToken;
        context.Customers.AddRange(
            new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Zelda",
                Email = "zelda@example.com",
                Phone = "1111-1111",
                BirthDate = new DateTime(1993, 7, 25)
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Alice",
                Email = "alice@example.com",
                Phone = "2222-2222",
                BirthDate = new DateTime(1990, 1, 15)
            });
        await context.SaveChangesAsync(ct);

        var handler = new Handler(context);

        var customers = await handler.GetAsync(ct);

        Assert.Collection(customers,
            first =>
            {
                Assert.Equal("Alice", first.Name);
                Assert.Equal("alice@example.com", first.Email);
            },
            second =>
            {
                Assert.Equal("Zelda", second.Name);
                Assert.Equal("zelda@example.com", second.Email);
            });
    }

    [Fact]
    public async Task CreateAsync_PersistsCustomerAndReturnsResponse()
    {
        using var context = TestDbContextFactory.CreateContext();
        var ct = TestContext.Current.CancellationToken;
        var handler = new Handler(context);
        var request = new BugStore.Requests.Customers.Create
        {
            Name = "Bruce Wayne",
            Email = "bruce@wayneenterprises.com",
            Phone = "3333-3333",
            BirthDate = new DateTime(1985, 2, 19)
        };

        var response = await handler.CreateAsync(request, ct);

        Assert.Equal(request.Name, response.Name);
        Assert.Equal(request.Email, response.Email);
        Assert.NotEqual(Guid.Empty, response.Id);

        var created = await context.Customers.SingleAsync(c => c.Id == response.Id, ct);
        Assert.Equal(request.Phone, created.Phone);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNullWhenCustomerDoesNotExist()
    {
        using var context = TestDbContextFactory.CreateContext();
        var ct = TestContext.Current.CancellationToken;
        var handler = new Handler(context);
        var request = new BugStore.Requests.Customers.Update
        {
            Name = "Updated Name",
            Email = "updated@example.com",
            Phone = "4444-4444",
            BirthDate = new DateTime(1995, 3, 10)
        };

        var response = await handler.UpdateAsync(Guid.NewGuid(), request, ct);

        Assert.Null(response);
    }

    [Fact]
    public async Task DeleteAsync_RemovesCustomerAndReturnsTrue()
    {
        using var context = TestDbContextFactory.CreateContext();
        var ct = TestContext.Current.CancellationToken;
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = "Clark Kent",
            Email = "clark@dailyplanet.com",
            Phone = "5555-5555",
            BirthDate = new DateTime(1982, 6, 18)
        };
        context.Customers.Add(customer);
        await context.SaveChangesAsync(ct);
        var handler = new Handler(context);

        var result = await handler.DeleteAsync(customer.Id, ct);

        Assert.True(result);
        Assert.False(await context.Customers.AnyAsync(c => c.Id == customer.Id, ct));
    }
}
