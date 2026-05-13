using APIForms.Domain.Entities;

namespace APIForms.Application.Interfaces;

public interface ISmartQlPolicyRepository
{
    Task<SmartQlPolicy?> GetByPolicyIdAsync(string tenantId, string formId, string policyId, CancellationToken ct);
    Task<List<SmartQlPolicy>> ListByEventAsync(string tenantId, string formId, string eventName, CancellationToken ct);
    Task UpsertAsync(SmartQlPolicy policy, CancellationToken ct);
}
