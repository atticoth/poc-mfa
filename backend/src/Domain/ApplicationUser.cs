using Microsoft.AspNetCore.Identity;

namespace PocMfa.Domain;

public class ApplicationUser : IdentityUser
{
    public bool TwoFactorEnabledApp { get; set; }
    public string? TwoFactorSecret { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
