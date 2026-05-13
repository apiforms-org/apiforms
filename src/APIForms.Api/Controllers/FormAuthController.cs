using System.Security.Cryptography;
using System.Text;
using APIForms.Api.Security;
using APIForms.Application.DTOs;
using APIForms.Application.Interfaces;
using APIForms.Application.Services;
using APIForms.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIForms.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/form-auth")]
public sealed class FormAuthController(FormService forms, IApiKeyRepository apiKeys, IFormRepository formRepo) : ControllerBase
{
    [HttpGet("{formId}")]
    public async Task<IActionResult> Get(string formId, CancellationToken ct)
    {
        var tenantId = TenantContext.GetTenantId(HttpContext);
        var form = await forms.GetByIdAsync(tenantId, formId, ct);
        if (form is null) return NotFound();

        var keys = await apiKeys.ListAsync(tenantId, formId, ct);
        var key = keys.FirstOrDefault(x => x.IsActive);
        return Ok(new FormAuthSettingsDto
        {
            RequireJwt = form.Settings.JwtRequired,
            RequireSubscriptionKey = form.Settings.ApiKeyRequired,
            HasActiveKey = keys.Any(x => x.IsActive),
            KeyPreview = key?.KeyPreview
        });
    }

    [HttpPut("{formId}")]
    public async Task<IActionResult> Update(string formId, [FromBody] UpdateFormAuthSettingsRequest req, CancellationToken ct)
    {
        var tenantId = TenantContext.GetTenantId(HttpContext);
        var form = await forms.GetByIdAsync(tenantId, formId, ct);
        if (form is null) return NotFound();

        form.Settings.JwtRequired = req.RequireJwt;
        form.Settings.ApiKeyRequired = req.RequireSubscriptionKey;
        await formRepo.UpdateAsync(form, ct);

        var keys = await apiKeys.ListAsync(tenantId, formId, ct);
        var key = keys.FirstOrDefault(x => x.IsActive);
        return Ok(new FormAuthSettingsDto
        {
            RequireJwt = form.Settings.JwtRequired,
            RequireSubscriptionKey = form.Settings.ApiKeyRequired,
            HasActiveKey = keys.Any(x => x.IsActive),
            KeyPreview = key?.KeyPreview
        });
    }

    [HttpGet("{formId}/keys")]
    public async Task<IActionResult> ListKeys(string formId, CancellationToken ct)
    {
        var tenantId = TenantContext.GetTenantId(HttpContext);
        var form = await forms.GetByIdAsync(tenantId, formId, ct);
        if (form is null) return NotFound();

        var keys = await apiKeys.ListAsync(tenantId, formId, ct);
        return Ok(keys.Select(k => new SubscriptionKeyItemDto
        {
            Id = k.Id,
            Name = k.Name,
            KeyPreview = k.KeyPreview,
            IsActive = k.IsActive,
            CreatedAt = k.CreatedAt
        }).ToList());
    }

    [HttpPost("{formId}/keys")]
    public async Task<IActionResult> CreateKey(string formId, [FromBody] CreateSubscriptionKeyRequest req, CancellationToken ct)
    {
        var tenantId = TenantContext.GetTenantId(HttpContext);
        var form = await forms.GetByIdAsync(tenantId, formId, ct);
        if (form is null) return NotFound();

        var name = req.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.");

        var existing = await apiKeys.ListAsync(tenantId, formId, ct);
        if (existing.Any(x => x.IsActive && string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"An active key with name '{name}' already exists.");

        var raw = $"sk_{Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant()}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();
        var keyId = Guid.NewGuid().ToString("N");
        var keyPreview = raw[..10] + "...";

        await apiKeys.CreateAsync(new ApiFormApiKey
        {
            Id = keyId,
            TenantId = tenantId,
            FormId = formId,
            Name = name,
            KeyHash = hash,
            KeyPreview = keyPreview,
            IsActive = true
        }, ct);

        return Ok(new CreateSubscriptionKeyResponse { Id = keyId, Name = name, Key = raw, KeyPreview = keyPreview });
    }

    [HttpDelete("{formId}/keys/{keyId}")]
    public async Task<IActionResult> RevokeKey(string formId, string keyId, CancellationToken ct)
    {
        var tenantId = TenantContext.GetTenantId(HttpContext);
        var form = await forms.GetByIdAsync(tenantId, formId, ct);
        if (form is null) return NotFound();

        return await apiKeys.DeactivateAsync(tenantId, formId, keyId, ct) ? NoContent() : NotFound();
    }
}
