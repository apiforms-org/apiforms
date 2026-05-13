namespace APIForms.Domain.Entities;

public sealed class ApiFormUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string PasswordSalt { get; set; }
    public required string TenantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
