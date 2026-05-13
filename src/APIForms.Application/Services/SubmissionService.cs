using APIForms.Application.DTOs;
using APIForms.Application.Interfaces;
using APIForms.Domain.Entities;
using System.Globalization;
using System.Text.Json;

namespace APIForms.Application.Services;

public sealed class SubmissionService(IFormRepository forms, ISubmissionRepository submissions, IPermissionRepository permissions, SmartQlGatewayService smartQlGateway)
{
    public async Task<ApiFormSubmission> SubmitPublicAsync(string slug, PublicSubmitRequest req, CancellationToken ct)
    {
        var form = await forms.GetPublishedBySlugAsync(slug.Trim().ToLowerInvariant(), ct)
            ?? throw new KeyNotFoundException("Published form not found");

        var permission = await permissions.GetAsync(form.TenantId, form.Id, ct);
        if (permission is not null && !permission.PublicSubmit) throw new UnauthorizedAccessException("Public submit disabled");

        ValidateAnswers(form.Fields, req.Answers);
        var normalizedAnswers = NormalizeAnswers(req.Answers);
        normalizedAnswers = await smartQlGateway.ExecuteAsync(form.TenantId, form.Id, "form.submit", normalizedAnswers, ct);

        return await submissions.CreateAsync(new ApiFormSubmission
        {
            TenantId = form.TenantId,
            FormId = form.Id,
            Answers = normalizedAnswers
        }, ct);
    }

    public async Task<ApiFormSubmission> SubmitPublicAsync(string formId, string slug, PublicSubmitRequest req, CancellationToken ct)
    {
        var form = await forms.GetPublishedByIdAndSlugAsync(formId, slug.Trim().ToLowerInvariant(), ct)
            ?? throw new KeyNotFoundException("Published form not found");

        var permission = await permissions.GetAsync(form.TenantId, form.Id, ct);
        if (permission is not null && !permission.PublicSubmit) throw new UnauthorizedAccessException("Public submit disabled");

        ValidateAnswers(form.Fields, req.Answers);
        var normalizedAnswers = NormalizeAnswers(req.Answers);
        normalizedAnswers = await smartQlGateway.ExecuteAsync(form.TenantId, form.Id, "form.submit", normalizedAnswers, ct);

        return await submissions.CreateAsync(new ApiFormSubmission
        {
            TenantId = form.TenantId,
            FormId = form.Id,
            Answers = normalizedAnswers
        }, ct);
    }

    public async Task<List<ApiFormSubmission>> GetDataAsync(string tenantId, string slug, CancellationToken ct)
    {
        var form = await forms.GetBySlugAsync(tenantId, slug.Trim().ToLowerInvariant(), ct)
            ?? throw new KeyNotFoundException("Form not found");
        await EnsureCrudPermissionAsync(tenantId, form.Id, PermissionAction.Read, ct);

        return await submissions.ListByFormIdAsync(tenantId, form.Id, ct);
    }

    public async Task<List<ApiFormSubmission>> GetDataAsync(string tenantId, string formId, string slug, CancellationToken ct)
    {
        var form = await forms.GetByIdAsync(tenantId, formId, ct);
        if (form is null || !string.Equals(form.Slug, slug.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
            throw new KeyNotFoundException("Form not found");
        await EnsureCrudPermissionAsync(tenantId, form.Id, PermissionAction.Read, ct);
        return await submissions.ListByFormIdAsync(tenantId, form.Id, ct);
    }

    public async Task<ApiFormSubmission> CreateDataAsync(string tenantId, string slug, PublicSubmitRequest req, CancellationToken ct)
    {
        var form = await forms.GetBySlugAsync(tenantId, slug.Trim().ToLowerInvariant(), ct)
            ?? throw new KeyNotFoundException("Form not found");
        await EnsureCrudPermissionAsync(tenantId, form.Id, PermissionAction.Create, ct);

        ValidateAnswers(form.Fields, req.Answers);
        var normalizedAnswers = NormalizeAnswers(req.Answers);
        normalizedAnswers = await smartQlGateway.ExecuteAsync(tenantId, form.Id, "api.create", normalizedAnswers, ct);

        return await submissions.CreateAsync(new ApiFormSubmission
        {
            TenantId = tenantId,
            FormId = form.Id,
            Answers = normalizedAnswers
        }, ct);
    }

    public async Task<ApiFormSubmission> CreateDataAsync(string tenantId, string formId, string slug, PublicSubmitRequest req, CancellationToken ct)
    {
        var form = await forms.GetByIdAsync(tenantId, formId, ct);
        if (form is null || !string.Equals(form.Slug, slug.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
            throw new KeyNotFoundException("Form not found");
        await EnsureCrudPermissionAsync(tenantId, form.Id, PermissionAction.Create, ct);
        ValidateAnswers(form.Fields, req.Answers);
        var normalizedAnswers = NormalizeAnswers(req.Answers);
        normalizedAnswers = await smartQlGateway.ExecuteAsync(tenantId, form.Id, "api.create", normalizedAnswers, ct);
        return await submissions.CreateAsync(new ApiFormSubmission
        {
            TenantId = tenantId,
            FormId = form.Id,
            Answers = normalizedAnswers
        }, ct);
    }

    public async Task<ApiFormSubmission?> GetDataByIdAsync(string tenantId, string slug, string id, CancellationToken ct)
    {
        var form = await forms.GetBySlugAsync(tenantId, slug.Trim().ToLowerInvariant(), ct);
        if (form is null) return null;
        await EnsureCrudPermissionAsync(tenantId, form.Id, PermissionAction.Read, ct);

        return await submissions.GetByIdAsync(tenantId, form.Id, id, ct);
    }

    public async Task<ApiFormSubmission?> GetDataByIdAsync(string tenantId, string formId, string slug, string id, CancellationToken ct)
    {
        var form = await forms.GetByIdAsync(tenantId, formId, ct);
        if (form is null || !string.Equals(form.Slug, slug.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
            return null;
        await EnsureCrudPermissionAsync(tenantId, form.Id, PermissionAction.Read, ct);
        return await submissions.GetByIdAsync(tenantId, form.Id, id, ct);
    }

    public async Task<List<ApiFormSubmission>> SearchDataAsync(string tenantId, string formId, string slug, string fieldId, string value, CancellationToken ct)
    {
        var form = await forms.GetByIdAsync(tenantId, formId, ct);
        if (form is null || !string.Equals(form.Slug, slug.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
            throw new KeyNotFoundException("Form not found");

        await EnsureCrudPermissionAsync(tenantId, form.Id, PermissionAction.Read, ct);

        var normalizedField = fieldId.Trim();
        if (string.IsNullOrWhiteSpace(normalizedField))
            throw new InvalidOperationException("Field is required.");

        var fieldExists = form.Fields.Any(f => string.Equals(f.Id, normalizedField, StringComparison.OrdinalIgnoreCase));
        if (!fieldExists)
            throw new InvalidOperationException($"Field '{normalizedField}' does not exist in this form.");

        return await submissions.SearchByAnswerAsync(tenantId, form.Id, normalizedField, value, ct);
    }

    public async Task<List<ApiFormSubmission>> SearchDataByQuestionAndFieldAsync(
        string tenantId,
        string formId,
        string slug,
        string? question,
        Dictionary<string, string> filters,
        CancellationToken ct)
    {
        var form = await forms.GetByIdAsync(tenantId, formId, ct);
        if (form is null || !string.Equals(form.Slug, slug.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
            throw new KeyNotFoundException("Form not found");

        await EnsureCrudPermissionAsync(tenantId, form.Id, PermissionAction.Read, ct);

        var normalizedQuestion = question?.Trim();

        if (filters.Count == 0)
            throw new ArgumentException("At least one field=value filter is required.");

        var normalizedFilters = filters
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Key))
            .ToDictionary(
                kv => kv.Key.Trim(),
                kv => kv.Value?.Trim() ?? string.Empty,
                StringComparer.OrdinalIgnoreCase);

        foreach (var filterField in normalizedFilters.Keys)
        {
            var fieldExists = form.Fields.Any(f => string.Equals(f.Id, filterField, StringComparison.OrdinalIgnoreCase));
            if (!fieldExists)
                throw new InvalidOperationException($"Field '{filterField}' does not exist in this form.");
        }

        var all = await submissions.ListByFormIdAsync(tenantId, form.Id, ct);
        var filtered = all
            .Where(s => normalizedFilters.All(filter =>
                s.Answers.TryGetValue(filter.Key, out var answerValue) &&
                string.Equals(answerValue?.ToString()?.Trim(), filter.Value, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (string.IsNullOrWhiteSpace(normalizedQuestion))
            return filtered;

        return filtered
            .Where(s => s.Answers.Values.Any(v => (v?.ToString() ?? string.Empty)
                .Contains(normalizedQuestion, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    public async Task<ApiFormSubmission?> UpdateDataAsync(string tenantId, string slug, string id, PublicSubmitRequest req, CancellationToken ct)
    {
        var form = await forms.GetBySlugAsync(tenantId, slug.Trim().ToLowerInvariant(), ct);
        if (form is null) return null;
        await EnsureCrudPermissionAsync(tenantId, form.Id, PermissionAction.Update, ct);

        ValidateAnswers(form.Fields, req.Answers);
        var normalizedAnswers = NormalizeAnswers(req.Answers);
        normalizedAnswers = await smartQlGateway.ExecuteAsync(tenantId, form.Id, "api.update", normalizedAnswers, ct);

        var existing = await submissions.GetByIdAsync(tenantId, form.Id, id, ct);
        if (existing is null) return null;

        existing.Answers = normalizedAnswers;
        await submissions.UpdateAsync(existing, ct);
        return existing;
    }

    public async Task<ApiFormSubmission?> UpdateDataAsync(string tenantId, string formId, string slug, string id, PublicSubmitRequest req, CancellationToken ct)
    {
        var form = await forms.GetByIdAsync(tenantId, formId, ct);
        if (form is null || !string.Equals(form.Slug, slug.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
            return null;
        await EnsureCrudPermissionAsync(tenantId, form.Id, PermissionAction.Update, ct);
        ValidateAnswers(form.Fields, req.Answers);
        var normalizedAnswers = NormalizeAnswers(req.Answers);
        normalizedAnswers = await smartQlGateway.ExecuteAsync(tenantId, form.Id, "api.update", normalizedAnswers, ct);
        var existing = await submissions.GetByIdAsync(tenantId, form.Id, id, ct);
        if (existing is null) return null;
        existing.Answers = normalizedAnswers;
        await submissions.UpdateAsync(existing, ct);
        return existing;
    }

    public async Task<bool> DeleteDataAsync(string tenantId, string slug, string id, CancellationToken ct)
    {
        var form = await forms.GetBySlugAsync(tenantId, slug.Trim().ToLowerInvariant(), ct);
        if (form is null) return false;
        await EnsureCrudPermissionAsync(tenantId, form.Id, PermissionAction.Delete, ct);

        return await submissions.DeleteAsync(tenantId, form.Id, id, ct);
    }

    public async Task<bool> DeleteDataAsync(string tenantId, string formId, string slug, string id, CancellationToken ct)
    {
        var form = await forms.GetByIdAsync(tenantId, formId, ct);
        if (form is null || !string.Equals(form.Slug, slug.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
            return false;
        await EnsureCrudPermissionAsync(tenantId, form.Id, PermissionAction.Delete, ct);
        return await submissions.DeleteAsync(tenantId, form.Id, id, ct);
    }

    private async Task EnsureCrudPermissionAsync(string tenantId, string formId, PermissionAction action, CancellationToken ct)
    {
        var permission = await permissions.GetAsync(tenantId, formId, ct);
        if (permission is null) return;

        var allowed = action switch
        {
            PermissionAction.Create => permission.Create,
            PermissionAction.Read => permission.Read,
            PermissionAction.Update => permission.Update,
            PermissionAction.Delete => permission.Delete,
            _ => false
        };

        if (!allowed)
        {
            throw new UnauthorizedAccessException($"Permission '{action}' is disabled for this form.");
        }
    }

    private static void ValidateAnswers(List<ApiFormField> fields, Dictionary<string, object?> answers)
    {
        foreach (var field in fields)
        {
            answers.TryGetValue(field.Id, out var value);

            if (field.Required && (value is null || string.IsNullOrWhiteSpace(value.ToString())))
            {
                throw new InvalidOperationException($"Field '{field.Id}' is required");
            }

            if (value is not null && !IsValidForType(field, value))
            {
                throw new InvalidOperationException($"Field '{field.Id}' has invalid value for type '{field.Type}'.");
            }
        }
    }

    private static bool IsValidForType(ApiFormField field, object value)
    {
        var type = field.Type.Trim().ToLowerInvariant();
        var text = value.ToString() ?? string.Empty;

        return type switch
        {
            "text" => true,
            "textarea" => true,
            "email" => text.Contains('@'),
            "number" => long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out _),
            "decimal" => decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out _),
            "date" => DateTime.TryParse(text, out _),
            "datetime" => DateTime.TryParse(text, out _),
            "boolean" => bool.TryParse(text, out _),
            "select" => field.Options.Count == 0 || field.Options.Contains(text),
            _ => true
        };
    }

    private static Dictionary<string, object?> NormalizeAnswers(Dictionary<string, object?> raw)
    {
        var normalized = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in raw)
        {
            normalized[kv.Key] = NormalizeValue(kv.Value);
        }
        return normalized;
    }

    private static object? NormalizeValue(object? value)
    {
        if (value is null) return null;
        if (value is JsonElement je) return NormalizeJsonElement(je);
        return value;
    }

    private static object? NormalizeJsonElement(JsonElement el)
    {
        return el.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.Undefined => null,
            JsonValueKind.String => el.GetString(),
            JsonValueKind.Number => el.TryGetInt64(out var l) ? l :
                                    el.TryGetDecimal(out var d) ? d :
                                    el.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Array => el.EnumerateArray().Select(NormalizeJsonElement).ToList(),
            JsonValueKind.Object => el.EnumerateObject()
                .ToDictionary(p => p.Name, p => NormalizeJsonElement(p.Value)),
            _ => el.ToString()
        };
    }
}

public enum PermissionAction
{
    Create,
    Read,
    Update,
    Delete
}
