using APIForms.Domain.Entities;

namespace APIForms.Application.Interfaces;

public interface IPermissionRepository
{
    Task<ApiFormPermission?> GetAsync(string tenantId, string formId, CancellationToken ct);
    Task UpsertAsync(ApiFormPermission permission, CancellationToken ct);
}
