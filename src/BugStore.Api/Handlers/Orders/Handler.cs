using BugStore.Data;
using BugStore.Models;
using Microsoft.EntityFrameworkCore;
using CreateOrderRequest = BugStore.Requests.Orders.Create;
using CreateOrderResponse = BugStore.Responses.Orders.Create;
using GetOrderByIdResponse = BugStore.Responses.Orders.GetById;
using OrderLineResponse = BugStore.Responses.Orders.Line;

namespace BugStore.Handlers.Orders;

public class Handler(AppDbContext context)
{

    public async Task<GetOrderByIdResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await context.Orders
            .AsNoTracking()
            .Include(o => o.Lines)
            .ThenInclude(line => line.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order is null)
            return null;

        return MapOrder(order);
    }

    public async Task<CreateOrderResponse?> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Lines is null || request.Lines.Count == 0 || request.Lines.Any(line => line.Quantity <= 0))
            return null;

        var customerExists = await context.Customers
            .AsNoTracking()
            .AnyAsync(customer => customer.Id == request.CustomerId, cancellationToken);
        if (!customerExists)
            return null;

        var productIds = request.Lines
            .Select(line => line.ProductId)
            .Distinct()
            .ToList();

        var products = await context.Products
            .Where(product => productIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, cancellationToken);

        if (products.Count != productIds.Count)
            return null;

        var now = DateTime.UtcNow;
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            CreatedAt = now,
            UpdatedAt = now
        };

        foreach (var line in request.Lines)
        {
            var product = products[line.ProductId];
            var lineTotal = product.Price * line.Quantity;

            order.Lines.Add(new OrderLine
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = product.Id,
                Quantity = line.Quantity,
                Total = lineTotal
            });
        }

        context.Orders.Add(order);
        await context.SaveChangesAsync(cancellationToken);

        var responseLines = order.Lines
            .Select(line => new OrderLineResponse(
                line.Id,
                line.ProductId,
                products[line.ProductId].Title,
                line.Quantity,
                line.Total))
            .ToList();

        var total = responseLines.Sum(line => line.Total);

        return new CreateOrderResponse(
            order.Id,
            order.CustomerId,
            total,
            order.CreatedAt,
            order.UpdatedAt,
            responseLines);
    }

    private static GetOrderByIdResponse MapOrder(Order order)
    {
        var responseLines = order.Lines
            .Select(line => new OrderLineResponse(
                line.Id,
                line.ProductId,
                line.Product?.Title ?? string.Empty,
                line.Quantity,
                line.Total))
            .ToList();

        var total = responseLines.Sum(line => line.Total);

        return new GetOrderByIdResponse(
            order.Id,
            order.CustomerId,
            total,
            order.CreatedAt,
            order.UpdatedAt,
            responseLines);
    }
}
