using IdentitySample.WebApp.Data;
using Microsoft.EntityFrameworkCore;

namespace IdentitySample.WebApp.Services;

public interface IUserRepository
{
    Task<User?> GetUserAsync(string? userName, CancellationToken cancellationToken = default);
}

public class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _identityDbContext;
    public UserRepository(IdentityDbContext identityDbContext)
    {
        _identityDbContext = identityDbContext;
    }
    public async Task<User?> GetUserAsync(string? userName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userName))
            return null;
        return await _identityDbContext
            .Users
            .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken: cancellationToken);
    }
}