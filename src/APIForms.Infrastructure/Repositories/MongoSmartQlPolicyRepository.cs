using APIForms.Application.Interfaces;
using APIForms.Domain.Entities;
using MongoDB.Driver;

namespace APIForms.Infrastructure.Repositories;

public sealed class MongoSmartQlPolicyRepository(IMongoDatabase db) : ISmartQlPolicyRepository
{
    private readonly IMongoCollection<SmartQlPolicy> _policies = db.GetCollection<SmartQlPolicy>("smartql_policies");

    public async Task<SmartQlPolicy?> GetByPolicyIdAsync(string tenantId, string formId, string policyId, CancellationToken ct)
        => await _policies.Find(x => x.TenantId == tenantId && x.FormId == formId && x.PolicyId == policyId).FirstOrDefaultAsync(ct);

    public Task<List<SmartQlPolicy>> ListByEventAsync(string tenantId, string formId, string eventName, CancellationToken ct)
        => _policies.Find(x => x.TenantId == tenantId && x.FormId == formId && x.EventName == eventName && x.Enabled)
            .SortBy(x => x.Priority)
            .ThenBy(x => x.UpdatedAt)
            .ToListAsync(ct);

    public async Task UpsertAsync(SmartQlPolicy policy, CancellationToken ct)
    {
        await _policies.ReplaceOneAsync(
            x => x.TenantId == policy.TenantId && x.FormId == policy.FormId && x.PolicyId == policy.PolicyId,
            policy,
            new ReplaceOptions { IsUpsert = true },
            ct);
    }
}
