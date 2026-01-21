namespace DocumentSearch.Api.Models;

public class Document
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
