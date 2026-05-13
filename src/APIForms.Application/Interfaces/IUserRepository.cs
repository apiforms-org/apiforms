using APIForms.Domain.Entities;

namespace APIForms.Application.Interfaces;

public interface IUserRepository
{
    Task<ApiFormUser?> GetByEmailAsync(string email, CancellationToken ct);
    Task<ApiFormUser> CreateAsync(ApiFormUser user, CancellationToken ct);
}
