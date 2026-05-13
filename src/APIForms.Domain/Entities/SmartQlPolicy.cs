namespace APIForms.Domain.Entities;

public sealed class SmartQlPolicy
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public required string TenantId { get; set; }
    public required string FormId { get; set; }
    public required string PolicyId { get; set; }
    public required string EventName { get; set; }
    public required string Script { get; set; }
    public bool Enabled { get; set; } = true;
    public int Priority { get; set; } = 100;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
