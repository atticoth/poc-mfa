using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PocMfa.Domain;
using PocMfa.Infrastructure;

namespace PocMfa.Application;

public interface IRefreshTokenService
{
    Task<RefreshToken> CreateAsync(ApplicationUser user, string refreshToken, string ipAddress);
    Task<RefreshToken?> GetActiveTokenAsync(string refreshToken);
    Task RotateAsync(RefreshToken existing, string newTokenHash, string ipAddress);
    Task RevokeAsync(RefreshToken existing, string ipAddress);
}

public class RefreshTokenService : IRefreshTokenService
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwt;
    private readonly JwtSettings _settings;

    public RefreshTokenService(AppDbContext db, IJwtTokenService jwt, IOptions<JwtSettings> options)
    {
        _db = db;
        _jwt = jwt;
        _settings = options.Value;
    }

    public async Task<RefreshToken> CreateAsync(ApplicationUser user, string refreshToken, string ipAddress)
    {
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = _jwt.HashToken(refreshToken),
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_settings.RefreshTokenDays),
            CreatedByIp = ipAddress
        };

        _db.RefreshTokens.Add(token);
        await _db.SaveChangesAsync();
        return token;
    }

    public async Task<RefreshToken?> GetActiveTokenAsync(string refreshToken)
    {
        var hash = _jwt.HashToken(refreshToken);
        return await _db.RefreshTokens.Include(rt => rt.User)
            .SingleOrDefaultAsync(rt => rt.TokenHash == hash && rt.RevokedAt == null && rt.ExpiresAt > DateTimeOffset.UtcNow);
    }

    public async Task RotateAsync(RefreshToken existing, string newTokenHash, string ipAddress)
    {
        existing.RevokedAt = DateTimeOffset.UtcNow;
        existing.ReplacedByTokenHash = newTokenHash;
        existing.CreatedByIp = ipAddress;
        _db.RefreshTokens.Update(existing);
        await _db.SaveChangesAsync();
    }

    public async Task RevokeAsync(RefreshToken existing, string ipAddress)
    {
        existing.RevokedAt = DateTimeOffset.UtcNow;
        existing.CreatedByIp = ipAddress;
        _db.RefreshTokens.Update(existing);
        await _db.SaveChangesAsync();
    }
}
