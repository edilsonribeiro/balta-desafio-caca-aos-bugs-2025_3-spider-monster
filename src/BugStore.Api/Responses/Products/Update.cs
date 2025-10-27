namespace BugStore.Responses.Products;

public record Update(Guid Id, string Title, string Description, string Slug, decimal Price);
