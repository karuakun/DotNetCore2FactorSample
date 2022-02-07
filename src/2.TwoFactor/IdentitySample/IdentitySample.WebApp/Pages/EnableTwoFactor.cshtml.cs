using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using IdentitySample.WebApp.Data;
using IdentitySample.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OtpNet;

namespace IdentitySample.WebApp.Pages;

[Authorize]
public class EnableTwoFactorModel : PageModel
{
    public string? AuthenticatorUri { get; private set; }
    public string? Secrets { get; private set; }
    [BindProperty] public string? VerifyCode { get; set; }
    public string? Message { get; set; }
    
    private readonly IUserRepository _userRepository;
    private readonly UrlEncoder _urlEncoder;

    public EnableTwoFactorModel(IUserRepository userRepository, UrlEncoder urlEncoder)
    {
        _userRepository = userRepository;
        _urlEncoder = urlEncoder;
    }

    public async Task<IActionResult> OnGet()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return NotFound();
        await SetPageModelFromCurrentUserAsync(user);
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return NotFound();
        await SetPageModelFromCurrentUserAsync(user);

        var totp = new Totp(Base32Encoding.ToBytes(Secrets), totpSize: 6);
        if (!totp.VerifyTotp(VerifyCode, out _, VerificationWindow.RfcSpecifiedNetworkDelay))
        {
            Message = "検証に失敗しました。もう一度入力してください。";
            return Page();
        }

        Message = "検証に成功しました。";
        await _userRepository.SetUseTwoFactorAsync(user.UserName, true);
        return Page();
    }

    private async Task<User?> GetCurrentUserAsync()
    {
        var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userName))
            return null;
        return await _userRepository.GetUserAsync(userName);
    }
    private async Task SetPageModelFromCurrentUserAsync(User user)
    {
        var secrets = await GetOrGenerateTwoFactorSecretsAsync(user);
        AuthenticatorUri = GenerateQrCodeUri("test-app", user.UserName, secrets);
        Secrets = secrets;
    }
    private async Task<string> GetOrGenerateTwoFactorSecretsAsync(User user)
    {
        if (!string.IsNullOrEmpty(user.TwoFactorSecrets)) 
            return user.TwoFactorSecrets;

        var key = KeyGeneration.GenerateRandomKey();
        var secrets = Base32Encoding.ToString(key);

        user.TwoFactorSecrets = secrets;
        await _userRepository.SetTwoFactorTokenAsync(user.UserName, user.TwoFactorSecrets);
        return user.TwoFactorSecrets;
    }
    
    private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
    private string GenerateQrCodeUri(string appName, string userName, string unformattedSecrets)
    {
        return string.Format(
            AuthenticatorUriFormat,
            _urlEncoder.Encode(appName),
            _urlEncoder.Encode(userName),
            unformattedSecrets);
    }
}
