using APIForms.Application.DTOs;
using APIForms.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIForms.Api.Controllers;

[ApiController]
[Route("api/forms/public")]
public sealed class PublicFormsController(FormService forms, SubmissionService submissions) : ControllerBase
{
    [HttpGet("{formId}/{slug}")]
    public async Task<IActionResult> Get(string formId, string slug, CancellationToken ct)
    {
        var form = await forms.GetPublicByIdAndSlugAsync(formId, slug, ct);
        return form is null ? NotFound() : Ok(form);
    }

    [Authorize]
    [HttpPost("{formId}/{slug}/submit")]
    public async Task<IActionResult> Submit(string formId, string slug, [FromBody] PublicSubmitRequest req, CancellationToken ct)
    {
        var created = await submissions.SubmitPublicAsync(formId, slug, req, ct);
        return Ok(created);
    }
}
