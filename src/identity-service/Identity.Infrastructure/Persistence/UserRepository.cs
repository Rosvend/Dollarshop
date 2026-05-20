using Identity.Domain.Aggregates;
using Identity.Domain.Interfaces;
using Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence;

public sealed class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _dbContext;

    public UserRepository(IdentityDbContext dbContext) => _dbContext = dbContext;

    public async Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default) =>
        await _dbContext.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public async Task<User?> GetByUsernameAsync(Username username, CancellationToken cancellationToken = default) =>
        await _dbContext.Users.FirstOrDefaultAsync(user => user.Username == username, cancellationToken);

    public async Task<bool> ExistsByUsernameAsync(Username username, CancellationToken cancellationToken = default) =>
        await _dbContext.Users.AnyAsync(user => user.Username == username, cancellationToken);

    public async Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default) =>
        await _dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken);

    public Task SaveAsync(User user, CancellationToken cancellationToken = default)
    {
        if (_dbContext.Entry(user).State == EntityState.Detached)
        {
            _dbContext.Users.Add(user);
        }

        return Task.CompletedTask;
    }
}
