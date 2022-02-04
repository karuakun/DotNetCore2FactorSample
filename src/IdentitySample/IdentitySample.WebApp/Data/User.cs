namespace IdentitySample.WebApp.Data;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public bool UseTwoFactor { get; set; }
    public virtual IList<TwoFactorToken> TwoFactorTokens { get; set; } = null!;
}