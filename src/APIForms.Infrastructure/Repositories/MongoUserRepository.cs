using APIForms.Application.Interfaces;
using APIForms.Domain.Entities;
using MongoDB.Driver;

namespace APIForms.Infrastructure.Repositories;

public sealed class MongoUserRepository(IMongoDatabase db) : IUserRepository
{
    private readonly IMongoCollection<ApiFormUser> _users = db.GetCollection<ApiFormUser>("api_form_users");

    public async Task<ApiFormUser> CreateAsync(ApiFormUser user, CancellationToken ct)
    {
        await _users.InsertOneAsync(user, cancellationToken: ct);
        return user;
    }

    public async Task<ApiFormUser?> GetByEmailAsync(string email, CancellationToken ct)
        => await _users.Find(x => x.Email == email).FirstOrDefaultAsync(ct);
}
