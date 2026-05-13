using APIForms.Domain.Entities;

namespace APIForms.Application.DTOs;

public sealed class CreateFormRequest
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public List<ApiFormField> Fields { get; set; } = [];
    public ApiFormSettings? Settings { get; set; }
}

public sealed class UpdateFormRequest
{
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public List<ApiFormField>? Fields { get; set; }
    public ApiFormSettings? Settings { get; set; }
}

public sealed class PublicSubmitRequest
{
    public Dictionary<string, object?> Answers { get; set; } = [];
}
