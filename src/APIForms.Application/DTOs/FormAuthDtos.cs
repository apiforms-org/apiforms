namespace APIForms.Application.DTOs;

public sealed class FormAuthSettingsDto
{
    public bool RequireJwt { get; set; }
    public bool RequireSubscriptionKey { get; set; }
    public bool HasActiveKey { get; set; }
    public string? KeyPreview { get; set; }
}

public sealed class UpdateFormAuthSettingsRequest
{
    public bool RequireJwt { get; set; }
    public bool RequireSubscriptionKey { get; set; }
}

public sealed class CreateSubscriptionKeyResponse
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Key { get; set; }
    public required string KeyPreview { get; set; }
}

public sealed class CreateSubscriptionKeyRequest
{
    public required string Name { get; set; }
}

public sealed class SubscriptionKeyItemDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string KeyPreview { get; set; }
    public required bool IsActive { get; set; }
    public required DateTime CreatedAt { get; set; }
}
