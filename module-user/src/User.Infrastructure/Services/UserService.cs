using Microsoft.EntityFrameworkCore;
using User.Application.Services;
using User.Infrastructure.TenantDb;

namespace User.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserDbContext _dbContext;

    public UserService(UserDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> CreateUserAsync(string username, string email, string role)
    {
        var user = new Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            Role = role
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user.Id;
    }

    public async Task<Domain.Entities.User?> GetUserAsync(Guid id)
    {
        return await _dbContext.Users.FindAsync(id);
    }

    public async Task<IEnumerable<Domain.Entities.User>> GetAllUsersAsync()
    {
        return await _dbContext.Users.ToListAsync();
    }

    public async Task UpdateUserAsync(Guid id, string username, string email, string role)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user != null)
        {
            user.Username = username;
            user.Email = email;
            user.Role = role;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user != null)
        {
            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
        }
    }
}
