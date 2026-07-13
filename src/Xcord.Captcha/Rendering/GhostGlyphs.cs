using System.Drawing;

namespace Xcord.Captcha.Rendering;

public static class GhostGlyphs
{
    private static PointF P(float col, float row) => new(col / 2f, row / 4f);

    // Polylines per glyph, authored on a 3-column x 5-row grid (col in [0,2], row in [0,4])
    // mapped into the unit box [0,1]x[0,1] via P(col,row). Every char in CaptchaCharset.Chars
    // must have an entry here.
    private static readonly Dictionary<char, PointF[][]> Table = new()
    {
        ['A'] = new[] { new[] { P(0,4), P(1,0), P(2,4) }, new[] { P(0.4f,2.2f), P(1.6f,2.2f) } },
        ['C'] = new[]
        {
            new[] { P(2,0.6f), P(0.7f,0), P(0,1.2f), P(0,2.8f), P(0.7f,4), P(2,3.4f) },
        },
        ['D'] = new[]
        {
            new[] { P(0,4), P(0,0), P(1.1f,0), P(2,1.2f), P(2,2.8f), P(1.1f,4), P(0,4) },
        },
        ['E'] = new[]
        {
            new[] { P(0,0), P(0,4) },
            new[] { P(0,0), P(2,0) },
            new[] { P(0,2), P(1.6f,2) },
            new[] { P(0,4), P(2,4) },
        },
        ['F'] = new[]
        {
            new[] { P(0,0), P(0,4) },
            new[] { P(0,0), P(2,0) },
            new[] { P(0,2), P(1.6f,2) },
        },
        ['H'] = new[] { new[] { P(0,0), P(0,4) }, new[] { P(2,0), P(2,4) }, new[] { P(0,2), P(2,2) } },
        ['J'] = new[]
        {
            new[] { P(0.6f,0), P(2,0) },
            new[] { P(1.6f,0), P(1.6f,3), P(1,4), P(0.2f,3.4f) },
        },
        ['K'] = new[]
        {
            new[] { P(0,0), P(0,4) },
            new[] { P(0,2), P(2,0) },
            new[] { P(0,2), P(2,4) },
        },
        ['M'] = new[]
        {
            new[] { P(0,4), P(0,0), P(1,2.2f), P(2,0), P(2,4) },
        },
        ['N'] = new[]
        {
            new[] { P(0,4), P(0,0), P(2,4), P(2,0) },
        },
        ['P'] = new[]
        {
            new[] { P(0,4), P(0,0), P(1.4f,0), P(2,0.8f), P(1.4f,1.6f), P(0,1.6f) },
        },
        ['R'] = new[]
        {
            new[] { P(0,4), P(0,0), P(1.4f,0), P(2,0.8f), P(1.4f,1.6f), P(0,1.6f) },
            new[] { P(0.8f,1.6f), P(2,4) },
        },
        ['T'] = new[]
        {
            new[] { P(0,0), P(2,0) },
            new[] { P(1,0), P(1,4) },
        },
        ['U'] = new[]
        {
            new[] { P(0,0), P(0,3), P(0.5f,4), P(1.5f,4), P(2,3), P(2,0) },
        },
        ['V'] = new[]
        {
            new[] { P(0,0), P(1,4), P(2,0) },
        },
        ['W'] = new[]
        {
            new[] { P(0,0), P(0.6f,4), P(1,1.6f), P(1.4f,4), P(2,0) },
        },
        ['X'] = new[]
        {
            new[] { P(0,0), P(2,4) },
            new[] { P(2,0), P(0,4) },
        },
        ['Y'] = new[]
        {
            new[] { P(0,0), P(1,2) },
            new[] { P(2,0), P(1,2), P(1,4) },
        },
        ['3'] = new[]
        {
            new[] { P(0,0.3f), P(1.6f,0), P(2,1), P(1,2), P(2,3), P(1.6f,4), P(0,3.7f) },
        },
        ['4'] = new[] { new[] { P(1.6f,0), P(0,2.6f), P(2,2.6f) }, new[] { P(1.6f,0), P(1.6f,4) } },
        ['6'] = new[]
        {
            new[] { P(1.8f,0), P(0.6f,0.3f), P(0,1.5f), P(0,3.2f), P(0.8f,4), P(1.8f,3.7f), P(2,2.7f), P(1.2f,2), P(0.2f,2.3f) },
        },
        ['7'] = new[] { new[] { P(0,0), P(2,0), P(0.9f,4) } },
        ['9'] = new[]
        {
            new[] { P(0.6f,0), P(1.7f,0.2f), P(2,1.1f), P(1.7f,2), P(0.6f,2.2f), P(0,1.4f), P(0.3f,0.4f), P(0.6f,0) },
            new[] { P(1.8f,1.8f), P(1.6f,3), P(0.8f,4) },
        },
    };

    public static IReadOnlyList<IReadOnlyList<PointF>> Strokes(char c)
    {
        var key = char.ToUpperInvariant(c);
        if (!Table.TryGetValue(key, out var strokes))
            throw new ArgumentOutOfRangeException(nameof(c), $"No glyph for '{c}'");
        return strokes;
    }
}
