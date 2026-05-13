namespace APIForms.Domain.Entities;

public sealed class ApiFormApiKey
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public required string TenantId { get; set; }
    public required string FormId { get; set; }
    public required string Name { get; set; }
    public required string KeyHash { get; set; }
    public required string KeyPreview { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
