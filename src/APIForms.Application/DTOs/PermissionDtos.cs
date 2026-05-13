namespace APIForms.Application.DTOs;

public sealed class FormPermissionDto
{
    public bool Create { get; set; }
    public bool Read { get; set; }
    public bool Update { get; set; }
    public bool Delete { get; set; }
    public bool PublicSubmit { get; set; }
}
