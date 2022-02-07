using System.Security.Claims;
using IdentitySample.WebApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OtpNet;

namespace IdentitySample.WebApp.Pages;

public class LoginModel : PageModel
{
    [BindProperty] public string? UserName { get; set; }
    [BindProperty] public string? Password { get; set; }
    [BindProperty] public string? VerifyCode { get; set; }
    public string? Message { get; set; }

    private readonly IUserRepository _userRepository;
    
    public LoginModel(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public void OnGet() { }

    public async Task OnPostAsync(string returnUrl)
    {
        var user = await _userRepository.GetUserAsync(UserName);
        if (user == null || user.Password != Password)
        {
            Message = "ログインに失敗しました。";            
            return;
        }

        if (user.UseTwoFactor)
        {
            var totp = new Totp(Base32Encoding.ToBytes(user.TwoFactorSecrets), totpSize: 6);
            if (!totp.VerifyTotp(VerifyCode, out _, VerificationWindow.RfcSpecifiedNetworkDelay))
            {
                Message = "ログインに失敗しました。";
                return;
            }
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserName),
            new("amr", user.UseTwoFactor ? "mfa": "pwd")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        LocalRedirect(returnUrl);
    }
}
