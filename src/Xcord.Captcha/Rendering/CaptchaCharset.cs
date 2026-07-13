using System.Security.Cryptography;

namespace Xcord.Captcha.Rendering;

public static class CaptchaCharset
{
    // A-Z + 2-9 minus look-alikes (no O/0, I/1/L, S/5, Z/2, B/8, G/6, Q).
    public const string Chars = "ACDEFHJKMNPRTUVWXY34679";
    public const int DefaultLength = 5;

    public static string NewCode(int length = DefaultLength)
    {
        if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
        var chars = new char[length];
        for (var i = 0; i < length; i++)
            chars[i] = Chars[RandomNumberGenerator.GetInt32(Chars.Length)];
        return new string(chars);
    }
}
