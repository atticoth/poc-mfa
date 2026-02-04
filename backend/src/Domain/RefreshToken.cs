namespace PocMfa.Domain;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string CreatedByIp { get; set; } = string.Empty;
    public string? ReplacedByTokenHash { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public bool IsActive => RevokedAt == null && DateTimeOffset.UtcNow <= ExpiresAt;
}
