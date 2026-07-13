using System.Security.Cryptography;

namespace Xcord.Captcha.Rendering;

public static class CaptchaCharset
{
    // A-Z + 2-9 minus look-alikes. Excluded: letters G I L O Q S Z B and digits 0 1 2 5 8.
    // 6 is kept: its only look-alike (G) is already excluded, so it is unambiguous in this set.
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
