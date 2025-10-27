namespace BugStore.Requests.Products;

public record Get(int Page = 1, int PageSize = 25);
