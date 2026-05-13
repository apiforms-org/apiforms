using APIForms.Application.Interfaces;
using APIForms.Domain.Entities;
using MongoDB.Driver;

namespace APIForms.Infrastructure.Repositories;

public sealed class MongoFormRepository(IMongoDatabase db) : IFormRepository
{
    private readonly IMongoCollection<ApiForm> _forms = db.GetCollection<ApiForm>("api_forms");

    public async Task<ApiForm> CreateAsync(ApiForm form, CancellationToken ct)
    {
        await _forms.InsertOneAsync(form, cancellationToken: ct);
        return form;
    }

    public async Task<bool> DeleteAsync(string tenantId, string id, CancellationToken ct)
    {
        var res = await _forms.DeleteOneAsync(x => x.TenantId == tenantId && x.Id == id, ct);
        return res.DeletedCount > 0;
    }

    public async Task<ApiForm?> GetByIdAsync(string tenantId, string id, CancellationToken ct)
        => await _forms.Find(x => x.TenantId == tenantId && x.Id == id).FirstOrDefaultAsync(ct);

    public async Task<ApiForm?> GetBySlugAsync(string tenantId, string slug, CancellationToken ct)
        => await _forms.Find(x => x.TenantId == tenantId && x.Slug == slug).FirstOrDefaultAsync(ct);

    public async Task<ApiForm?> GetByIdAndSlugAsync(string formId, string slug, CancellationToken ct)
        => await _forms.Find(x => x.Id == formId && x.Slug == slug).FirstOrDefaultAsync(ct);

    public async Task<ApiForm?> GetPublishedBySlugAsync(string slug, CancellationToken ct)
        => await _forms.Find(x => x.Slug == slug && x.Status == "published").FirstOrDefaultAsync(ct);

    public async Task<ApiForm?> GetPublishedByIdAndSlugAsync(string formId, string slug, CancellationToken ct)
        => await _forms.Find(x => x.Id == formId && x.Slug == slug && x.Status == "published").FirstOrDefaultAsync(ct);

    public Task<List<ApiForm>> ListAsync(string tenantId, CancellationToken ct)
        => _forms.Find(x => x.TenantId == tenantId).ToListAsync(ct);

    public async Task<bool> UpdateAsync(ApiForm form, CancellationToken ct)
    {
        var res = await _forms.ReplaceOneAsync(x => x.TenantId == form.TenantId && x.Id == form.Id, form, cancellationToken: ct);
        return res.ModifiedCount > 0;
    }
}
