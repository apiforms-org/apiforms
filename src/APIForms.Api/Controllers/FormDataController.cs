using APIForms.Api.Security;
using APIForms.Application.DTOs;
using APIForms.Application.Interfaces;
using APIForms.Application.Services;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace APIForms.Api.Controllers;

[ApiController]
[Route("api/forms/{formId}/{slug}/data")]
public sealed class FormDataController(SubmissionService submissions, IFormRepository forms, IApiKeyRepository apiKeys) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(string formId, string slug, CancellationToken ct)
    {
        var tenantId = await ResolveTenantAsync(formId, slug, ct);
        return Ok(await submissions.GetDataAsync(tenantId, formId, slug, ct));
    }

    [HttpPost]
    public async Task<IActionResult> Create(string formId, string slug, [FromBody] PublicSubmitRequest req, CancellationToken ct)
    {
        var tenantId = await ResolveTenantAsync(formId, slug, ct);
        return Ok(await submissions.CreateDataAsync(tenantId, formId, slug, req, ct));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string formId, string slug, string id, CancellationToken ct)
    {
        var tenantId = await ResolveTenantAsync(formId, slug, ct);
        var result = await submissions.GetDataByIdAsync(tenantId, formId, slug, id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(string formId, string slug, [FromQuery] string field, [FromQuery] string value, CancellationToken ct)
    {
        var tenantId = await ResolveTenantAsync(formId, slug, ct);
        return Ok(await submissions.SearchDataAsync(tenantId, formId, slug, field, value, ct));
    }

    [HttpGet("search-by-question")]
    public async Task<IActionResult> SearchByQuestion(string formId, string slug, [FromQuery] string? question, [FromQuery] string? filters, CancellationToken ct)
    {
        var queryPairs = Request.Query
            .Where(q => !string.Equals(q.Key, "question", StringComparison.OrdinalIgnoreCase))
            .Where(q => !string.Equals(q.Key, "filters", StringComparison.OrdinalIgnoreCase))
            .Where(q => q.Value.Count > 0 && !string.IsNullOrWhiteSpace(q.Value.ToString()))
            .ToList();

        var parsedFilters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var pair in queryPairs)
            parsedFilters[pair.Key] = pair.Value.ToString().Trim();

        if (!string.IsNullOrWhiteSpace(filters))
        {
            var chunks = filters.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var chunk in chunks)
            {
                var parts = chunk.Split(':', 2, StringSplitOptions.TrimEntries);
                if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
                    throw new ArgumentException("Invalid 'filters' format. Use: filters=nombre:diego,ciudad:cali");

                parsedFilters[parts[0]] = parts[1];
            }
        }

        if (parsedFilters.Count == 0)
            throw new ArgumentException("You must provide at least one filter. Example: ?question=hola&nombre=diego or ?question=hola&filters=nombre:diego,ciudad:cali");

        var tenantId = await ResolveTenantAsync(formId, slug, ct);
        return Ok(await submissions.SearchDataByQuestionAndFieldAsync(tenantId, formId, slug, question, parsedFilters, ct));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string formId, string slug, string id, [FromBody] PublicSubmitRequest req, CancellationToken ct)
    {
        var tenantId = await ResolveTenantAsync(formId, slug, ct);
        var result = await submissions.UpdateDataAsync(tenantId, formId, slug, id, req, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string formId, string slug, string id, CancellationToken ct)
    {
        var tenantId = await ResolveTenantAsync(formId, slug, ct);
        return await submissions.DeleteDataAsync(tenantId, formId, slug, id, ct) ? NoContent() : NotFound();
    }

    private async Task<string> ResolveTenantAsync(string formId, string slug, CancellationToken ct)
    {
        var form = await forms.GetByIdAndSlugAsync(formId, slug.Trim().ToLowerInvariant(), ct)
            ?? throw new KeyNotFoundException("Form not found");

        var jwtTenant = User.FindFirstValue("tenantId");
        var hasJwt = !string.IsNullOrWhiteSpace(jwtTenant);

        if (form.Settings.JwtRequired && !hasJwt)
            throw new UnauthorizedAccessException("JWT required for this API.");

        if (hasJwt && !string.Equals(jwtTenant, form.TenantId, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("Invalid tenant access.");

        if (form.Settings.ApiKeyRequired)
        {
            if (!Request.Headers.TryGetValue("x-api-key", out var keyHeader) || string.IsNullOrWhiteSpace(keyHeader.ToString()))
                throw new UnauthorizedAccessException("Subscription key required (x-api-key).");

            var incomingHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(keyHeader.ToString()))).ToLowerInvariant();
            var matchingKey = await apiKeys.GetActiveByHashAsync(form.TenantId, form.Id, incomingHash, ct);
            if (matchingKey is null)
                throw new UnauthorizedAccessException("Invalid subscription key.");
        }

        return form.TenantId;
    }
}
