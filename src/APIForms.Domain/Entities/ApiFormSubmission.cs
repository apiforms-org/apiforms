namespace APIForms.Domain.Entities;

public sealed class ApiFormSubmission
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public required string TenantId { get; set; }
    public required string FormId { get; set; }
    public Dictionary<string, object?> Answers { get; set; } = [];
    public Dictionary<string, object?> Metadata { get; set; } = [];
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}
