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
[Route("api/form-permissions")]
public sealed class FormPermissionsController(IPermissionRepository permissions) : ControllerBase
{
    [HttpGet("{formId}")]
    public async Task<IActionResult> Get(string formId, CancellationToken ct)
    {
        var tenantId = TenantContext.GetTenantId(HttpContext);
        var permission = await permissions.GetAsync(tenantId, formId, ct) ?? new ApiFormPermission
        {
            TenantId = tenantId,
            FormId = formId
        };

        return Ok(new FormPermissionDto
        {
            Create = permission.Create,
            Read = permission.Read,
            Update = permission.Update,
            Delete = permission.Delete,
            PublicSubmit = permission.PublicSubmit
        });
    }

    [HttpPut("{formId}")]
    public async Task<IActionResult> Update(string formId, [FromBody] FormPermissionDto req, CancellationToken ct)
    {
        var tenantId = TenantContext.GetTenantId(HttpContext);
        var permission = await permissions.GetAsync(tenantId, formId, ct) ?? new ApiFormPermission
        {
            TenantId = tenantId,
            FormId = formId
        };

        permission.Create = req.Create;
        permission.Read = req.Read;
        permission.Update = req.Update;
        permission.Delete = req.Delete;
        permission.PublicSubmit = req.PublicSubmit;

        await permissions.UpsertAsync(permission, ct);
        return Ok(req);
    }
}
