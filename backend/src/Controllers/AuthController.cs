using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PocMfa.Application;
using PocMfa.Domain;

namespace PocMfa.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string RefreshCookieName = "refreshToken";
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwt;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ITotpService _totpService;
    private readonly IEmailService _emailService;
    private readonly IAuditLogService _auditLogService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwt,
        IRefreshTokenService refreshTokenService,
        ITotpService totpService,
        IEmailService emailService,
        IAuditLogService auditLogService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwt = jwt;
        _refreshTokenService = refreshTokenService;
        _totpService = totpService;
        _emailService = emailService;
        _auditLogService = auditLogService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        await _auditLogService.LogAsync(user.Id, "register", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "n/a");
        return Ok(new { message = "Usuário registrado" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || user.IsDeleted)
        {
            return Unauthorized();
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return Unauthorized();
        }

        if (user.TwoFactorEnabledApp)
        {
            return Ok(new { requiresTwoFactor = true, userId = user.Id });
        }

        return await IssueTokensAsync(user);
    }

    [HttpPost("login/2fa")]
    public async Task<IActionResult> LoginWithTwoFactor(TwoFactorRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null || string.IsNullOrEmpty(user.TwoFactorSecret))
        {
            return Unauthorized();
        }

        if (!_totpService.ValidateCode(user.TwoFactorSecret, request.Code))
        {
            return Unauthorized();
        }

        return await IssueTokensAsync(user);
    }

    [Authorize]
    [HttpPost("2fa/enable")]
    public async Task<IActionResult> EnableTwoFactor()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var secret = _totpService.GenerateSecret();
        user.TwoFactorSecret = secret;
        user.TwoFactorEnabledApp = true;
        await _userManager.UpdateAsync(user);

        var qrUri = _totpService.GenerateQrCodeUri(user.Email ?? user.UserName ?? "user", secret);
        return Ok(new { qrUri, secret });
    }

    [Authorize]
    [HttpPost("2fa/disable")]
    public async Task<IActionResult> DisableTwoFactor()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        user.TwoFactorSecret = null;
        user.TwoFactorEnabledApp = false;
        await _userManager.UpdateAsync(user);

        return Ok(new { message = "2FA desabilitado" });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies[RefreshCookieName];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized();
        }

        var existing = await _refreshTokenService.GetActiveTokenAsync(refreshToken);
        if (existing?.User == null)
        {
            return Unauthorized();
        }

        var newRefresh = _jwt.GenerateRefreshToken();
        var newHash = _jwt.HashToken(newRefresh);
        await _refreshTokenService.RotateAsync(existing, newHash, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "n/a");
        await _refreshTokenService.CreateAsync(existing.User, newRefresh, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "n/a");

        SetRefreshCookie(newRefresh);
        var accessToken = _jwt.CreateAccessToken(existing.User, Array.Empty<Claim>());
        return Ok(new TokenResponse(accessToken));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[RefreshCookieName];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var existing = await _refreshTokenService.GetActiveTokenAsync(refreshToken);
            if (existing != null)
            {
                await _refreshTokenService.RevokeAsync(existing, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "n/a");
            }
        }

        Response.Cookies.Delete(RefreshCookieName);
        return Ok();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Ok();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var callbackUrl = $"{request.ResetBaseUrl}?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";
        await _emailService.SendPasswordResetAsync(user.Email!, callbackUrl);

        return Ok();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return BadRequest();
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok();
    }

    private async Task<IActionResult> IssueTokensAsync(ApplicationUser user)
    {
        var accessToken = _jwt.CreateAccessToken(user, Array.Empty<Claim>());
        var refresh = _jwt.GenerateRefreshToken();
        await _refreshTokenService.CreateAsync(user, refresh, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "n/a");
        await _auditLogService.LogAsync(user.Id, "login", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "n/a");
        SetRefreshCookie(refresh);

        return Ok(new TokenResponse(accessToken));
    }

    private void SetRefreshCookie(string refreshToken)
    {
        Response.Cookies.Append(RefreshCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }

    public record RegisterRequest(string Email, string Password);
    public record LoginRequest(string Email, string Password);
    public record TwoFactorRequest(string UserId, string Code);
    public record ForgotPasswordRequest(string Email, string ResetBaseUrl);
    public record ResetPasswordRequest(string Email, string Token, string NewPassword);
    public record TokenResponse(string AccessToken);
}
