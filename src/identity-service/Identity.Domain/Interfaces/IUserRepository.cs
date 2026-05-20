using Identity.Domain.Aggregates;
using Identity.Domain.ValueObjects;

namespace Identity.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default);

    Task<User?> GetByUsernameAsync(Username username, CancellationToken cancellationToken = default);

    Task<bool> ExistsByUsernameAsync(Username username, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default);

    Task SaveAsync(User user, CancellationToken cancellationToken = default);
}
