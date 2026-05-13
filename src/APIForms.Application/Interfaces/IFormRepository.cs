using APIForms.Domain.Entities;

namespace APIForms.Application.Interfaces;

public interface IFormRepository
{
    Task<List<ApiForm>> ListAsync(string tenantId, CancellationToken ct);
    Task<ApiForm?> GetByIdAsync(string tenantId, string id, CancellationToken ct);
    Task<ApiForm?> GetBySlugAsync(string tenantId, string slug, CancellationToken ct);
    Task<ApiForm?> GetByIdAndSlugAsync(string formId, string slug, CancellationToken ct);
    Task<ApiForm?> GetPublishedBySlugAsync(string slug, CancellationToken ct);
    Task<ApiForm?> GetPublishedByIdAndSlugAsync(string formId, string slug, CancellationToken ct);
    Task<ApiForm> CreateAsync(ApiForm form, CancellationToken ct);
    Task<bool> UpdateAsync(ApiForm form, CancellationToken ct);
    Task<bool> DeleteAsync(string tenantId, string id, CancellationToken ct);
}
