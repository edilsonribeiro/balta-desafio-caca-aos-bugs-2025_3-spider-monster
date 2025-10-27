namespace BugStore.Responses.Customers;

public record Update(Guid Id, string Name, string Email, string Phone, DateTime BirthDate);
