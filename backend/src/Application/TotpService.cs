using OtpNet;

namespace PocMfa.Application;

public interface ITotpService
{
    string GenerateSecret();
    string GenerateQrCodeUri(string email, string secret);
    bool ValidateCode(string secret, string code);
}

public class TotpService : ITotpService
{
    public string GenerateSecret()
    {
        var secret = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(secret);
    }

    public string GenerateQrCodeUri(string email, string secret)
    {
        var issuer = Uri.EscapeDataString("POC MFA");
        var account = Uri.EscapeDataString(email);
        return $"otpauth://totp/{issuer}:{account}?secret={secret}&issuer={issuer}&digits=6";
    }

    public bool ValidateCode(string secret, string code)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secret));
        return totp.VerifyTotp(code, out _, new VerificationWindow(2, 2));
    }
}
