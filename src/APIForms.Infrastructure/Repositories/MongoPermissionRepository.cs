using APIForms.Application.Interfaces;
using APIForms.Domain.Entities;
using MongoDB.Driver;

namespace APIForms.Infrastructure.Repositories;

public sealed class MongoPermissionRepository(IMongoDatabase db) : IPermissionRepository
{
    private readonly IMongoCollection<ApiFormPermission> _permissions = db.GetCollection<ApiFormPermission>("api_form_permissions");

    public async Task<ApiFormPermission?> GetAsync(string tenantId, string formId, CancellationToken ct)
        => await _permissions.Find(x => x.TenantId == tenantId && x.FormId == formId).FirstOrDefaultAsync(ct);

    public async Task UpsertAsync(ApiFormPermission permission, CancellationToken ct)
    {
        await _permissions.ReplaceOneAsync(
            x => x.TenantId == permission.TenantId && x.FormId == permission.FormId,
            permission,
            new ReplaceOptions { IsUpsert = true },
            ct);
    }
}
