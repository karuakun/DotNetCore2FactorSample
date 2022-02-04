namespace IdentitySample.WebApp.Data;

public class TwoFactorToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string TokenType { get; set; } = null!;
    public string Secrets { get; set; } = null!;
}