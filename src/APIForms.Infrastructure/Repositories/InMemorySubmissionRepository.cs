using APIForms.Application.Interfaces;
using APIForms.Domain.Entities;

namespace APIForms.Infrastructure.Repositories;

public sealed class InMemorySubmissionRepository : ISubmissionRepository
{
    private static readonly List<ApiFormSubmission> Store = [];

    public Task<ApiFormSubmission> CreateAsync(ApiFormSubmission submission, CancellationToken ct)
    {
        Store.Add(submission);
        return Task.FromResult(submission);
    }

    public Task<bool> DeleteAsync(string tenantId, string formId, string submissionId, CancellationToken ct)
    {
        var removed = Store.RemoveAll(x => x.TenantId == tenantId && x.FormId == formId && x.Id == submissionId) > 0;
        return Task.FromResult(removed);
    }

    public Task<ApiFormSubmission?> GetByIdAsync(string tenantId, string formId, string submissionId, CancellationToken ct)
        => Task.FromResult(Store.FirstOrDefault(x => x.TenantId == tenantId && x.FormId == formId && x.Id == submissionId));

    public Task<List<ApiFormSubmission>> ListByFormIdAsync(string tenantId, string formId, CancellationToken ct)
        => Task.FromResult(Store.Where(x => x.TenantId == tenantId && x.FormId == formId).ToList());

    public Task<List<ApiFormSubmission>> SearchByAnswerAsync(string tenantId, string formId, string fieldId, string value, CancellationToken ct)
    {
        var found = Store
            .Where(x => x.TenantId == tenantId && x.FormId == formId)
            .Where(x => x.Answers.TryGetValue(fieldId, out var answer) &&
                        string.Equals(answer?.ToString(), value, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return Task.FromResult(found);
    }

    public Task<bool> UpdateAsync(ApiFormSubmission submission, CancellationToken ct)
    {
        var idx = Store.FindIndex(x => x.TenantId == submission.TenantId && x.FormId == submission.FormId && x.Id == submission.Id);
        if (idx < 0) return Task.FromResult(false);
        Store[idx] = submission;
        return Task.FromResult(true);
    }
}
