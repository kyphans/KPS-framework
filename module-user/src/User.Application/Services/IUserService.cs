using User.Domain.Entities;

namespace User.Application.Services;

public interface IUserService
{
    Task<Guid> CreateUserAsync(string username, string email, string role);
    Task<Domain.Entities.User?> GetUserAsync(Guid id);
    Task<IEnumerable<Domain.Entities.User>> GetAllUsersAsync();
    Task UpdateUserAsync(Guid id, string username, string email, string role);
    Task DeleteUserAsync(Guid id);
}
