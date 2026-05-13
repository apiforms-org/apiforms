using APIForms.Api.Security;
using APIForms.Application.DTOs;
using APIForms.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIForms.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/forms")]
public sealed class FormsController(FormService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var tenantId = TenantContext.GetTenantId(HttpContext);
        return Ok(await service.ListAsync(tenantId, ct));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFormRequest req, CancellationToken ct)
    {
        var tenantId = TenantContext.GetTenantId(HttpContext);
        var created = await service.CreateAsync(tenantId, req, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var tenantId = TenantContext.GetTenantId(HttpContext);
        var form = await service.GetByIdAsync(tenantId, id, ct);
        return form is null ? NotFound() : Ok(form);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateFormRequest req, CancellationToken ct)
    {
        var tenantId = TenantContext.GetTenantId(HttpContext);
        var updated = await service.UpdateAsync(tenantId, id, req, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var tenantId = TenantContext.GetTenantId(HttpContext);
        return await service.DeleteAsync(tenantId, id, ct) ? NoContent() : NotFound();
    }

    [HttpPost("{id}/publish")]
    public async Task<IActionResult> Publish(string id, CancellationToken ct)
    {
        var tenantId = TenantContext.GetTenantId(HttpContext);
        var form = await service.PublishAsync(tenantId, id, ct);
        return form is null ? NotFound() : Ok(form);
    }

    [HttpPost("{id}/unpublish")]
    public async Task<IActionResult> Unpublish(string id, CancellationToken ct)
    {
        var tenantId = TenantContext.GetTenantId(HttpContext);
        var form = await service.UnpublishAsync(tenantId, id, ct);
        return form is null ? NotFound() : Ok(form);
    }

}
