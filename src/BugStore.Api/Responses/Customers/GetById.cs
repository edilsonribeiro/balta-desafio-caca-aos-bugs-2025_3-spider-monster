namespace BugStore.Responses.Customers;

public record GetById(Guid Id, string Name, string Email, string Phone, DateTime BirthDate);
