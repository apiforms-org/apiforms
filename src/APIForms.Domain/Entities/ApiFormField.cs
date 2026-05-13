namespace APIForms.Domain.Entities;

public sealed class ApiFormField
{
    public required string Id { get; set; }
    public required string Type { get; set; }
    public required string Label { get; set; }
    public string? Placeholder { get; set; }
    public bool Required { get; set; }
    public object? DefaultValue { get; set; }
    public string? Regex { get; set; }
    public double? Min { get; set; }
    public double? Max { get; set; }
    public bool Readonly { get; set; }
    public bool Hidden { get; set; }
    public List<string> Options { get; set; } = [];
}
