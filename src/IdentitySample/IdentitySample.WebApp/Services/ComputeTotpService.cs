using System.Security.Cryptography;
using SimpleBase;

namespace IdentitySample.WebApp.Services;

public class ComputeTotpService
{
    public static string GenerateTotpKey()
    {
        return Base32.Rfc4648.Encode(GenerateRandomKey());
    }
    public static byte[] GenerateRandomKey()
    {
        var bytes = new byte[20];
        RandomNumberGenerator.Fill(bytes);
        return bytes;
    }
    public static bool ValidateTotp(string? totpKey, string? inputCode)
    {
        if (string.IsNullOrEmpty(totpKey))
            return false;
        if (string.IsNullOrEmpty(inputCode))
            return false;
        if (!int.TryParse(inputCode, out var code))
            return false;

        // 前後2回分の時間のずれを許容する
        for (var i = -2; i <= 2; i++)
        {
            var computedTotp = ComputeTotp(totpKey, i);
            if (computedTotp == code)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Authenticator の TOTP 更新スパン（30秒）
    /// </summary>
    public const int TotpRefreshSpanSec = 30;

    public static long GetCurrentCounter(int window)
        => (GetUnixTimestamp() / TotpRefreshSpanSec) + window;

    //  https://tools.ietf.org/html/rfc6238#section-4
    public static int ComputeTotp(string totpKey, int window = 0)
        => ComputeHotp(totpKey, GetCurrentCounter(window));

    public static int ComputeHotp(string key, long counter)
    {
        var secrets = Base32.Rfc4648.Decode(key);
        using var sha1Hmac = new HMACSHA1(secrets.ToArray());

        // https://tools.ietf.org/html/rfc4226
        var counterBytes = BitConverter.GetBytes(counter);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(counterBytes, 0, counterBytes.Length);
        }

        var hash = sha1Hmac.ComputeHash(counterBytes);
        var offset = hash[^1] & 0xf;
        var binaryCode = ((hash[offset] & 0x7f) << 24)
                         | ((hash[offset + 1] & 0xff) << 16)
                         | ((hash[offset + 2] & 0xff) << 8)
                         | ((hash[offset + 3] & 0xff));

        // # of 0's = length of pin
        return binaryCode % 1000000;
    }
    public static long GetUnixTimestamp()
    {
        var delta = DateTimeOffset.UtcNow - DateTimeOffset.UnixEpoch;
        return (long)delta.TotalSeconds;
    }
}