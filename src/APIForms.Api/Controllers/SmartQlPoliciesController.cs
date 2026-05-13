using APIForms.Api.Security;
using APIForms.Application.DTOs;
using APIForms.Application.Interfaces;
using APIForms.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIForms.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/smartql-policies")]
public sealed class SmartQlPoliciesController(ISmartQlPolicyRepository policies) : ControllerBase
{
    [HttpGet("{formId}/{policyId}")]
    public async Task<IActionResult> Get(string formId, string policyId, CancellationToken ct)
    {
        var tenantId = TenantContext.GetTenantId(HttpContext);
        var policy = await policies.GetByPolicyIdAsync(tenantId, formId, policyId, ct);
        if (policy is null) return NotFound();

        return Ok(new SmartQlPolicyResponse
        {
            FormId = formId,
            PolicyId = policy.PolicyId,
            Event = policy.EventName,
            SmartQl = policy.Script,
            Enabled = policy.Enabled,
            Priority = policy.Priority,
            UpdatedAt = policy.UpdatedAt
        });
    }

    [HttpPut("{formId}")]
    public async Task<IActionResult> Upsert(string formId, [FromBody] UpsertSmartQlPolicyRequest req, CancellationToken ct)
    {
        var tenantId = TenantContext.GetTenantId(HttpContext);

        if (string.IsNullOrWhiteSpace(req.PolicyId) || !req.PolicyId.StartsWith("smartql_", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("El policyId debe iniciar con 'smartql_'.");

        if (string.IsNullOrWhiteSpace(req.Event) || !req.Event.StartsWith("ON ", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("El evento debe iniciar con 'ON '.");

        var existing = await policies.GetByPolicyIdAsync(tenantId, formId, req.PolicyId.Trim(), ct);
        var entity = existing ?? new SmartQlPolicy
        {
            TenantId = tenantId,
            FormId = formId,
            PolicyId = req.PolicyId.Trim(),
            EventName = "form.submit",
            Script = "ON form.submit\nRETURN input"
        };

        entity.EventName = req.Event.Trim().Replace("ON ", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
        entity.Script = req.SmartQl.Trim();
        entity.Enabled = req.Enabled;
        entity.Priority = req.Priority;
        entity.UpdatedAt = DateTime.UtcNow;

        await policies.UpsertAsync(entity, ct);

        return Ok(new SmartQlPolicyResponse
        {
            FormId = formId,
            PolicyId = entity.PolicyId,
            Event = $"ON {entity.EventName}",
            SmartQl = entity.Script,
            Enabled = entity.Enabled,
            Priority = entity.Priority,
            UpdatedAt = entity.UpdatedAt
        });
    }
}
