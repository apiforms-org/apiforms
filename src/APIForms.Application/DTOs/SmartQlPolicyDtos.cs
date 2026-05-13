namespace APIForms.Application.DTOs;

public sealed class UpsertSmartQlPolicyRequest
{
    public required string PolicyId { get; set; }
    public required string Event { get; set; }
    public required string SmartQl { get; set; }
    public bool Enabled { get; set; } = true;
    public int Priority { get; set; } = 100;
}

public sealed class SmartQlPolicyResponse
{
    public required string FormId { get; set; }
    public required string PolicyId { get; set; }
    public required string Event { get; set; }
    public required string SmartQl { get; set; }
    public bool Enabled { get; set; }
    public int Priority { get; set; }
    public required DateTime UpdatedAt { get; set; }
}
