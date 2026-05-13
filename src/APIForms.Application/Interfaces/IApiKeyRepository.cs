using APIForms.Domain.Entities;

namespace APIForms.Application.Interfaces;

public interface IApiKeyRepository
{
    Task<ApiFormApiKey?> GetActiveAsync(string tenantId, string formId, CancellationToken ct);
    Task<ApiFormApiKey?> GetActiveByHashAsync(string tenantId, string formId, string keyHash, CancellationToken ct);
    Task<List<ApiFormApiKey>> ListAsync(string tenantId, string formId, CancellationToken ct);
    Task CreateAsync(ApiFormApiKey key, CancellationToken ct);
    Task<bool> DeactivateAsync(string tenantId, string formId, string keyId, CancellationToken ct);
}
