using IdentitySample.WebApp.Data;
using Microsoft.EntityFrameworkCore;

namespace IdentitySample.WebApp.Services;

public interface IUserRepository
{
    Task<User?> GetUserAsync(string? userName, CancellationToken cancellationToken = default);
    Task SetUseTwoFactorAsync(string? userName, bool enable, CancellationToken cancellationToken = default);
    Task SetTwoFactorTokenAsync(string userName, string twoFactorSecrets, CancellationToken cancellationToken = default);
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
    public async Task SetUseTwoFactorAsync(string? userName, bool enable, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userName))
            throw new InvalidOperationException();

        var user = await GetUserAsync(userName, cancellationToken);
        if (user == null)
            throw new InvalidOperationException();
        
        user.UseTwoFactor = enable;
        await _identityDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SetTwoFactorTokenAsync(string userName, string twoFactorSecrets, CancellationToken cancellationToken = default)
    {
        var user = await GetUserAsync(userName, cancellationToken);
        if (user == null)
            throw new InvalidOperationException();
        user.TwoFactorSecrets = twoFactorSecrets;
        await _identityDbContext.SaveChangesAsync(cancellationToken);
    }
}