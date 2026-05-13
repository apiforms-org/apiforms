namespace APIForms.Domain.Entities;

public sealed class ApiFormPermission
{
    public string? Id { get; set; }
    public required string TenantId { get; set; }
    public required string FormId { get; set; }
    public bool Create { get; set; } = true;
    public bool Read { get; set; } = true;
    public bool Update { get; set; } = true;
    public bool Delete { get; set; } = true;
    public bool PublicSubmit { get; set; } = true;
}
