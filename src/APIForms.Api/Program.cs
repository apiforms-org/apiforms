using System.Text;
using APIForms.Api.Middleware;
using APIForms.Application.Services;
using APIForms.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

const string CorsPolicy = "Frontend";

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<FormService>();
builder.Services.AddScoped<SubmissionService>();
builder.Services.AddScoped<SmartQlGatewayService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "apiforms";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "apiforms-clients";
var jwtKey = builder.Configuration["Jwt:Key"] ?? "ChangeThisJwtKeyForProduction_AtLeast32Chars";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
