using APIForms.Application.Interfaces;
using APIForms.Domain.Entities;

namespace APIForms.Infrastructure.Repositories;

public sealed class InMemoryPermissionRepository : IPermissionRepository
{
    private static readonly List<ApiFormPermission> Store = [];

    public Task<ApiFormPermission?> GetAsync(string tenantId, string formId, CancellationToken ct)
        => Task.FromResult(Store.FirstOrDefault(x => x.TenantId == tenantId && x.FormId == formId));

    public Task UpsertAsync(ApiFormPermission permission, CancellationToken ct)
    {
        var idx = Store.FindIndex(x => x.TenantId == permission.TenantId && x.FormId == permission.FormId);
        if (idx < 0) Store.Add(permission);
        else Store[idx] = permission;

        return Task.CompletedTask;
    }
}
