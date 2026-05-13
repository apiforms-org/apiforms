using APIForms.Application.DTOs;
using APIForms.Application.Interfaces;
using APIForms.Domain.Entities;
using System.Globalization;
using System.Text;

namespace APIForms.Application.Services;

public sealed class FormService(IFormRepository forms, IPermissionRepository permissions)
{
    public Task<List<ApiForm>> ListAsync(string tenantId, CancellationToken ct) => forms.ListAsync(tenantId, ct);

    public async Task<ApiForm> CreateAsync(string tenantId, CreateFormRequest req, CancellationToken ct)
    {
        EnsureFieldIds(req.Fields);
        ValidateCreateRequest(req);
        var slug = NormalizeSlug(req.Slug);
        var exists = await forms.GetBySlugAsync(tenantId, slug, ct);
        if (exists is not null) throw new InvalidOperationException("Slug already exists for tenant");

        var form = new ApiForm
        {
            TenantId = tenantId,
            Name = req.Name.Trim(),
            Slug = slug,
            Fields = req.Fields,
            Settings = req.Settings ?? new ApiFormSettings()
        };

        return await forms.CreateAsync(form, ct);
    }

    public Task<ApiForm?> GetByIdAsync(string tenantId, string id, CancellationToken ct) => forms.GetByIdAsync(tenantId, id, ct);
    public Task<ApiForm?> GetBySlugAsync(string tenantId, string slug, CancellationToken ct) => forms.GetBySlugAsync(tenantId, NormalizeSlug(slug), ct);

    public async Task<ApiForm?> UpdateAsync(string tenantId, string id, UpdateFormRequest req, CancellationToken ct)
    {
        var current = await forms.GetByIdAsync(tenantId, id, ct);
        if (current is null) return null;

        if (!string.IsNullOrWhiteSpace(req.Slug) && req.Slug != current.Slug)
        {
            var newSlug = NormalizeSlug(req.Slug);
            var exists = await forms.GetBySlugAsync(tenantId, newSlug, ct);
            if (exists is not null && exists.Id != current.Id) throw new InvalidOperationException("Slug already exists for tenant");
            current.Slug = newSlug;
        }

        if (!string.IsNullOrWhiteSpace(req.Name)) current.Name = req.Name.Trim();
        if (req.Fields is not null) current.Fields = req.Fields;
        if (req.Settings is not null) current.Settings = req.Settings;
        current.UpdatedAt = DateTime.UtcNow;
        current.Version++;

        await forms.UpdateAsync(current, ct);
        return current;
    }

    public Task<bool> DeleteAsync(string tenantId, string id, CancellationToken ct) => forms.DeleteAsync(tenantId, id, ct);

    public async Task<ApiForm?> PublishAsync(string tenantId, string id, CancellationToken ct)
    {
        var form = await forms.GetByIdAsync(tenantId, id, ct);
        if (form is null) return null;
        form.Status = "published";
        form.UpdatedAt = DateTime.UtcNow;
        await forms.UpdateAsync(form, ct);

        await permissions.UpsertAsync(new ApiFormPermission
        {
            TenantId = tenantId,
            FormId = form.Id
        }, ct);

        return form;
    }

    public async Task<ApiForm?> UnpublishAsync(string tenantId, string id, CancellationToken ct)
    {
        var form = await forms.GetByIdAsync(tenantId, id, ct);
        if (form is null) return null;
        form.Status = "draft";
        form.UpdatedAt = DateTime.UtcNow;
        await forms.UpdateAsync(form, ct);
        return form;
    }

    public Task<ApiForm?> GetPublicBySlugAsync(string slug, CancellationToken ct) => forms.GetPublishedBySlugAsync(NormalizeSlug(slug), ct);
    public Task<ApiForm?> GetPublicByIdAndSlugAsync(string formId, string slug, CancellationToken ct)
        => forms.GetPublishedByIdAndSlugAsync(formId, NormalizeSlug(slug), ct);

    private static string NormalizeSlug(string slug)
    {
        return slug.Trim().ToLowerInvariant().Replace(" ", "-");
    }

    private static void ValidateCreateRequest(CreateFormRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
        {
            throw new ArgumentException("El nombre del formulario es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(req.Slug))
        {
            throw new ArgumentException("El slug del formulario es obligatorio.");
        }

        if (req.Fields.Count == 0)
        {
            throw new ArgumentException("Debes agregar al menos un campo.");
        }

        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var field in req.Fields)
        {
            if (string.IsNullOrWhiteSpace(field.Label))
            {
                throw new ArgumentException($"El campo '{field.Id}' debe tener label.");
            }

            if (!ids.Add(field.Id.Trim()))
            {
                throw new ArgumentException($"El id de campo '{field.Id}' está repetido.");
            }
        }
    }

    private static void EnsureFieldIds(List<ApiFormField> fields)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < fields.Count; i++)
        {
            var field = fields[i];
            var baseId = !string.IsNullOrWhiteSpace(field.Id) ? field.Id : field.Label;
            var candidate = ToFieldId(baseId);
            if (string.IsNullOrWhiteSpace(candidate))
            {
                candidate = $"field_{i + 1}";
            }

            var unique = candidate;
            var suffix = 2;
            while (!seen.Add(unique))
            {
                unique = $"{candidate}_{suffix}";
                suffix++;
            }

            field.Id = unique;
        }
    }

    private static string ToFieldId(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var normalized = input.Trim().ToLowerInvariant();
        var chars = normalized
            .Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .Select(c => char.IsLetterOrDigit(c) ? c : '_')
            .ToArray();

        var collapsed = new string(chars);
        while (collapsed.Contains("__", StringComparison.Ordinal))
        {
            collapsed = collapsed.Replace("__", "_", StringComparison.Ordinal);
        }

        return collapsed.Trim('_');
    }
}
