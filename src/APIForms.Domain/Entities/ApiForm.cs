namespace APIForms.Domain.Entities;

public sealed class ApiForm
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public required string TenantId { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string Status { get; set; } = "draft";
    public List<ApiFormField> Fields { get; set; } = [];
    public ApiFormSettings Settings { get; set; } = new();
    public int Version { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
