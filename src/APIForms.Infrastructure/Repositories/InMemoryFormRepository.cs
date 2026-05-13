using APIForms.Application.Interfaces;
using APIForms.Domain.Entities;

namespace APIForms.Infrastructure.Repositories;

public sealed class InMemoryFormRepository : IFormRepository
{
    private static readonly List<ApiForm> Store = [];

    public Task<ApiForm> CreateAsync(ApiForm form, CancellationToken ct)
    {
        Store.Add(form);
        return Task.FromResult(form);
    }

    public Task<bool> DeleteAsync(string tenantId, string id, CancellationToken ct)
    {
        var removed = Store.RemoveAll(x => x.TenantId == tenantId && x.Id == id) > 0;
        return Task.FromResult(removed);
    }

    public Task<ApiForm?> GetByIdAsync(string tenantId, string id, CancellationToken ct)
        => Task.FromResult(Store.FirstOrDefault(x => x.TenantId == tenantId && x.Id == id));

    public Task<ApiForm?> GetBySlugAsync(string tenantId, string slug, CancellationToken ct)
        => Task.FromResult(Store.FirstOrDefault(x => x.TenantId == tenantId && x.Slug == slug));

    public Task<ApiForm?> GetByIdAndSlugAsync(string formId, string slug, CancellationToken ct)
        => Task.FromResult(Store.FirstOrDefault(x => x.Id == formId && x.Slug == slug));

    public Task<ApiForm?> GetPublishedBySlugAsync(string slug, CancellationToken ct)
        => Task.FromResult(Store.FirstOrDefault(x => x.Slug == slug && x.Status == "published"));

    public Task<ApiForm?> GetPublishedByIdAndSlugAsync(string formId, string slug, CancellationToken ct)
        => Task.FromResult(Store.FirstOrDefault(x => x.Id == formId && x.Slug == slug && x.Status == "published"));

    public Task<List<ApiForm>> ListAsync(string tenantId, CancellationToken ct)
        => Task.FromResult(Store.Where(x => x.TenantId == tenantId).ToList());

    public Task<bool> UpdateAsync(ApiForm form, CancellationToken ct)
    {
        var idx = Store.FindIndex(x => x.Id == form.Id && x.TenantId == form.TenantId);
        if (idx < 0) return Task.FromResult(false);
        Store[idx] = form;
        return Task.FromResult(true);
    }
}
