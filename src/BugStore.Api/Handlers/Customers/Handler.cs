using BugStore.Data;
using BugStore.Models;
using Microsoft.EntityFrameworkCore;
using CreateCustomerRequest = BugStore.Requests.Customers.Create;
using UpdateCustomerRequest = BugStore.Requests.Customers.Update;
using CreateCustomerResponse = BugStore.Responses.Customers.Create;
using GetCustomerByIdResponse = BugStore.Responses.Customers.GetById;
using GetCustomersResponse = BugStore.Responses.Customers.Get;
using UpdateCustomerResponse = BugStore.Responses.Customers.Update;

namespace BugStore.Handlers.Customers;

public class Handler
{
    private readonly AppDbContext _context;

    public Handler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<GetCustomersResponse>> GetAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .AsNoTracking()
            .OrderBy(customer => customer.Name)
            .Select(customer => new GetCustomersResponse(
                customer.Id,
                customer.Name,
                customer.Email,
                customer.Phone,
                customer.BirthDate))
            .ToListAsync(cancellationToken);
    }

    public async Task<GetCustomerByIdResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(customer => customer.Id == id, cancellationToken);

        return customer is null
            ? null
            : new GetCustomerByIdResponse(
                customer.Id,
                customer.Name,
                customer.Email,
                customer.Phone,
                customer.BirthDate);
    }

    public async Task<CreateCustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            BirthDate = request.BirthDate
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateCustomerResponse(
            customer.Id,
            customer.Name,
            customer.Email,
            customer.Phone,
            customer.BirthDate);
    }

    public async Task<UpdateCustomerResponse?> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (customer is null)
            return null;

        customer.Name = request.Name;
        customer.Email = request.Email;
        customer.Phone = request.Phone;
        customer.BirthDate = request.BirthDate;

        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateCustomerResponse(
            customer.Id,
            customer.Name,
            customer.Email,
            customer.Phone,
            customer.BirthDate);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (customer is null)
            return false;

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
