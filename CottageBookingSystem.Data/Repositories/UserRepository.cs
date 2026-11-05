using CottageBookingSystem.Core.Entities;
using CottageBookingSystem.Core.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CottageBookingSystem.Data.Repositories;

public sealed class UserRepository(AppDbContext dbContext)
    : Repository<User>(dbContext), IUserRepository
{
    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        return DbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);
    }

    public async Task<bool> TryAddUserAsync(User user, CancellationToken cancellationToken = default)
    {
        await DbContext.Users.AddAsync(user, cancellationToken);

        try
        {
            await DbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (IsDuplicateKey(exception))
        {
            DbContext.Entry(user).State = EntityState.Detached;
            return false;
        }
    }

    private static bool IsDuplicateKey(DbUpdateException exception)
    {
        return exception.GetBaseException() is SqlException { Number: 2601 or 2627 };
    }
}
