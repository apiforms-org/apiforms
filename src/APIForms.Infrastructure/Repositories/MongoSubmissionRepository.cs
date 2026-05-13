using APIForms.Application.Interfaces;
using APIForms.Domain.Entities;
using MongoDB.Driver;
using MongoDB.Bson;

namespace APIForms.Infrastructure.Repositories;

public sealed class MongoSubmissionRepository(IMongoDatabase db) : ISubmissionRepository
{
    private readonly IMongoCollection<ApiFormSubmission> _submissions = db.GetCollection<ApiFormSubmission>("api_form_submissions");

    public async Task<ApiFormSubmission> CreateAsync(ApiFormSubmission submission, CancellationToken ct)
    {
        await _submissions.InsertOneAsync(submission, cancellationToken: ct);
        return submission;
    }

    public async Task<bool> DeleteAsync(string tenantId, string formId, string submissionId, CancellationToken ct)
    {
        var res = await _submissions.DeleteOneAsync(x => x.TenantId == tenantId && x.FormId == formId && x.Id == submissionId, ct);
        return res.DeletedCount > 0;
    }

    public async Task<ApiFormSubmission?> GetByIdAsync(string tenantId, string formId, string submissionId, CancellationToken ct)
        => await _submissions.Find(x => x.TenantId == tenantId && x.FormId == formId && x.Id == submissionId).FirstOrDefaultAsync(ct);

    public Task<List<ApiFormSubmission>> ListByFormIdAsync(string tenantId, string formId, CancellationToken ct)
        => _submissions.Find(x => x.TenantId == tenantId && x.FormId == formId).ToListAsync(ct);

    public Task<List<ApiFormSubmission>> SearchByAnswerAsync(string tenantId, string formId, string fieldId, string value, CancellationToken ct)
    {
        var baseFilter = Builders<ApiFormSubmission>.Filter.Eq(x => x.TenantId, tenantId) &
                         Builders<ApiFormSubmission>.Filter.Eq(x => x.FormId, formId);
        var fieldFilter = Builders<ApiFormSubmission>.Filter.Eq($"Answers.{fieldId}", BsonValue.Create(value));
        return _submissions.Find(baseFilter & fieldFilter).ToListAsync(ct);
    }

    public async Task<bool> UpdateAsync(ApiFormSubmission submission, CancellationToken ct)
    {
        var res = await _submissions.ReplaceOneAsync(
            x => x.TenantId == submission.TenantId && x.FormId == submission.FormId && x.Id == submission.Id,
            submission,
            cancellationToken: ct);
        return res.ModifiedCount > 0;
    }
}
