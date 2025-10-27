namespace BugStore.Requests.Customers;

public record Get(int Page = 1, int PageSize = 25);