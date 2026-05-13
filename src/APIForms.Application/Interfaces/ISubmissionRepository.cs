using APIForms.Domain.Entities;

namespace APIForms.Application.Interfaces;

public interface ISubmissionRepository
{
    Task<ApiFormSubmission> CreateAsync(ApiFormSubmission submission, CancellationToken ct);
    Task<List<ApiFormSubmission>> ListByFormIdAsync(string tenantId, string formId, CancellationToken ct);
    Task<List<ApiFormSubmission>> SearchByAnswerAsync(string tenantId, string formId, string fieldId, string value, CancellationToken ct);
    Task<ApiFormSubmission?> GetByIdAsync(string tenantId, string formId, string submissionId, CancellationToken ct);
    Task<bool> UpdateAsync(ApiFormSubmission submission, CancellationToken ct);
    Task<bool> DeleteAsync(string tenantId, string formId, string submissionId, CancellationToken ct);
}
