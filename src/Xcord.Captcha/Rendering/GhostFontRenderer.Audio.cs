namespace Xcord.Captcha.Rendering;

/// <summary>
/// Offline audio rendering of the captcha code: per-character spoken clips (embedded 16-bit PCM
/// WAV resources, generated offline so no runtime TTS dependency ships) concatenated with
/// additive background noise. This is the accessible fallback for users who cannot use the
/// motion GIF.
/// </summary>
public sealed partial class GhostFontRenderer
{
    private const int AudioSampleRate = 16000;

    public byte[] RenderWav(string code, int seed)
    {
        if (string.IsNullOrEmpty(code)) throw new ArgumentException("Code required", nameof(code));

        var rng = new Random(seed);
        var samples = new List<short>();

        AppendNoise(samples, AudioSampleRate / 4, rng, amplitude: 400); // lead-in
        foreach (var c in code)
        {
            var clip = LoadClipPcm(char.ToUpperInvariant(c));
            foreach (var s in clip)
            {
                var noisy = s + rng.Next(-500, 500);
                samples.Add((short)Math.Clamp(noisy, short.MinValue, short.MaxValue));
            }
            AppendNoise(samples, AudioSampleRate / 5, rng, amplitude: 400); // gap between letters
        }

        return WriteWav(samples, AudioSampleRate);
    }

    private static void AppendNoise(List<short> buf, int count, Random rng, int amplitude)
    {
        for (var i = 0; i < count; i++) buf.Add((short)rng.Next(-amplitude, amplitude));
    }

    // Loads a per-character embedded WAV and returns its 16-bit PCM samples. Parses the RIFF
    // chunk list to locate the "data" chunk (robust to metadata chunks like LIST/INFO), rather
    // than assuming a fixed 44-byte header.
    private static short[] LoadClipPcm(char c)
    {
        var asm = typeof(GhostFontRenderer).Assembly;
        var resourceName = asm.GetManifestResourceNames()
            .Single(n => n.EndsWith($".{c}.wav", StringComparison.Ordinal));
        using var stream = asm.GetManifestResourceStream(resourceName)!;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        var bytes = ms.ToArray();
        return ExtractPcm16(bytes);
    }

    private static short[] ExtractPcm16(byte[] wav)
    {
        // RIFF header: "RIFF" <size4> "WAVE", then a sequence of <id4><size4><payload>.
        if (wav.Length < 12 ||
            wav[0] != 'R' || wav[1] != 'I' || wav[2] != 'F' || wav[3] != 'F' ||
            wav[8] != 'W' || wav[9] != 'A' || wav[10] != 'V' || wav[11] != 'E')
            throw new InvalidDataException("Not a RIFF/WAVE stream");

        var pos = 12;
        while (pos + 8 <= wav.Length)
        {
            var id = System.Text.Encoding.ASCII.GetString(wav, pos, 4);
            var size = BitConverter.ToInt32(wav, pos + 4);
            var payload = pos + 8;
            if (id == "data")
            {
                var count = Math.Min(size, wav.Length - payload) / 2;
                var pcm = new short[count];
                Buffer.BlockCopy(wav, payload, pcm, 0, count * 2);
                return pcm;
            }
            pos = payload + size + (size & 1); // chunks are word-aligned
        }
        throw new InvalidDataException("No data chunk found in WAV");
    }

    private static byte[] WriteWav(IReadOnlyList<short> samples, int sampleRate)
    {
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);
        const int channels = 1;
        const int bitsPerSample = 16;
        var blockAlign = channels * bitsPerSample / 8;
        var byteRate = sampleRate * blockAlign;
        var dataBytes = samples.Count * 2;

        w.Write("RIFF"u8.ToArray());
        w.Write(36 + dataBytes);
        w.Write("WAVE"u8.ToArray());
        w.Write("fmt "u8.ToArray());
        w.Write(16);
        w.Write((short)1); // PCM
        w.Write((short)channels);
        w.Write(sampleRate);
        w.Write(byteRate);
        w.Write((short)blockAlign);
        w.Write((short)bitsPerSample);
        w.Write("data"u8.ToArray());
        w.Write(dataBytes);
        foreach (var s in samples) w.Write(s);
        w.Flush();
        return ms.ToArray();
    }
}
