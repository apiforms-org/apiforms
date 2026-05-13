using APIForms.Application.Interfaces;
using APIForms.Domain.Entities;
using MongoDB.Driver;

namespace APIForms.Infrastructure.Repositories;

public sealed class MongoApiKeyRepository(IMongoDatabase db) : IApiKeyRepository
{
    private readonly IMongoCollection<ApiFormApiKey> _keys = db.GetCollection<ApiFormApiKey>("api_form_api_keys");

    public async Task<ApiFormApiKey?> GetActiveAsync(string tenantId, string formId, CancellationToken ct)
        => await _keys.Find(x => x.TenantId == tenantId && x.FormId == formId && x.IsActive).FirstOrDefaultAsync(ct);

    public async Task<ApiFormApiKey?> GetActiveByHashAsync(string tenantId, string formId, string keyHash, CancellationToken ct)
        => await _keys.Find(x => x.TenantId == tenantId && x.FormId == formId && x.IsActive && x.KeyHash == keyHash).FirstOrDefaultAsync(ct);

    public Task<List<ApiFormApiKey>> ListAsync(string tenantId, string formId, CancellationToken ct)
        => _keys.Find(x => x.TenantId == tenantId && x.FormId == formId).SortByDescending(x => x.CreatedAt).ToListAsync(ct);

    public Task CreateAsync(ApiFormApiKey key, CancellationToken ct)
        => _keys.InsertOneAsync(key, cancellationToken: ct);

    public async Task<bool> DeactivateAsync(string tenantId, string formId, string keyId, CancellationToken ct)
    {
        var update = Builders<ApiFormApiKey>.Update.Set(x => x.IsActive, false);
        var res = await _keys.UpdateOneAsync(
            x => x.TenantId == tenantId && x.FormId == formId && x.Id == keyId && x.IsActive,
            update,
            cancellationToken: ct);
        return res.ModifiedCount > 0;
    }
}
