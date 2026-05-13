namespace APIForms.Domain.Entities;

public sealed class ApiFormSettings
{
    public bool ApiKeyRequired { get; set; }
    public bool JwtRequired { get; set; }
    public bool PublicRead { get; set; }
    public bool PublicWrite { get; set; } = true;
    public int RateLimit { get; set; } = 60;
    public bool Cors { get; set; }
    public List<string> AllowedOrigins { get; set; } = [];
}
