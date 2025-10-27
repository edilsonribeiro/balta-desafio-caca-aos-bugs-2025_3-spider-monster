using BugStore.Data;
using Microsoft.EntityFrameworkCore;
using CustomerHandler = BugStore.Handlers.Customers.Handler;
using CustomerCreateRequest = BugStore.Requests.Customers.Create;
using CustomerUpdateRequest = BugStore.Requests.Customers.Update;
using ProductHandler = BugStore.Handlers.Products.Handler;
using ProductCreateRequest = BugStore.Requests.Products.Create;
using ProductUpdateRequest = BugStore.Requests.Products.Update;
using OrderHandler = BugStore.Handlers.Orders.Handler;
using OrderCreateRequest = BugStore.Requests.Orders.Create;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=app.db"));
builder.Services.AddScoped<CustomerHandler>();
builder.Services.AddScoped<ProductHandler>();
builder.Services.AddScoped<OrderHandler>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/", () => "Hello World!");

app.MapGet("/v1/customers", async (CustomerHandler handler, CancellationToken cancellationToken) =>
{
    var customers = await handler.GetAsync(cancellationToken);
    return Results.Ok(customers);
});

app.MapGet("/v1/customers/{id:guid}", async (Guid id, CustomerHandler handler, CancellationToken cancellationToken) =>
{
    var customer = await handler.GetByIdAsync(id, cancellationToken);
    return customer is null
        ? Results.NotFound()
        : Results.Ok(customer);
});

app.MapPost("/v1/customers", async (CustomerCreateRequest request, CustomerHandler handler, CancellationToken cancellationToken) =>
{
    var customer = await handler.CreateAsync(request, cancellationToken);
    return Results.Created($"/v1/customers/{customer.Id}", customer);
});

app.MapPut("/v1/customers/{id:guid}", async (Guid id, CustomerUpdateRequest request, CustomerHandler handler, CancellationToken cancellationToken) =>
{
    var customer = await handler.UpdateAsync(id, request, cancellationToken);
    return customer is null
        ? Results.NotFound()
        : Results.Ok(customer);
});

app.MapDelete("/v1/customers/{id:guid}", async (Guid id, CustomerHandler handler, CancellationToken cancellationToken) =>
{
    var deleted = await handler.DeleteAsync(id, cancellationToken);
    return deleted
        ? Results.NoContent()
        : Results.NotFound();
});

app.MapGet("/v1/products", async (ProductHandler handler, CancellationToken cancellationToken) =>
{
    var products = await handler.GetAsync(cancellationToken);
    return Results.Ok(products);
});

app.MapGet("/v1/products/{id:guid}", async (Guid id, ProductHandler handler, CancellationToken cancellationToken) =>
{
    var product = await handler.GetByIdAsync(id, cancellationToken);
    return product is null
        ? Results.NotFound()
        : Results.Ok(product);
});

app.MapPost("/v1/products", async (ProductCreateRequest request, ProductHandler handler, CancellationToken cancellationToken) =>
{
    var product = await handler.CreateAsync(request, cancellationToken);
    return Results.Created($"/v1/products/{product.Id}", product);
});

app.MapPut("/v1/products/{id:guid}", async (Guid id, ProductUpdateRequest request, ProductHandler handler, CancellationToken cancellationToken) =>
{
    var product = await handler.UpdateAsync(id, request, cancellationToken);
    return product is null
        ? Results.NotFound()
        : Results.Ok(product);
});

app.MapDelete("/v1/products/{id:guid}", async (Guid id, ProductHandler handler, CancellationToken cancellationToken) =>
{
    var deleted = await handler.DeleteAsync(id, cancellationToken);
    return deleted
        ? Results.NoContent()
        : Results.NotFound();
});

app.MapGet("/v1/orders/{id:guid}", async (Guid id, OrderHandler handler, CancellationToken cancellationToken) =>
{
    var order = await handler.GetByIdAsync(id, cancellationToken);
    return order is null
        ? Results.NotFound()
        : Results.Ok(order);
});
app.MapPost("/v1/orders", async (OrderCreateRequest request, OrderHandler handler, CancellationToken cancellationToken) =>
{
    var order = await handler.CreateAsync(request, cancellationToken);
    return order is null
        ? Results.BadRequest()
        : Results.Created($"/v1/orders/{order.Id}", order);
});

app.Run();
