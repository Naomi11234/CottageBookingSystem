using CottageBookingSystem.Core.Entities;

namespace CottageBookingSystem.Core.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);
    Task<bool> TryAddUserAsync(User user, CancellationToken cancellationToken = default);
}
