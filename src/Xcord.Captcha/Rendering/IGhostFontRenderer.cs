namespace Xcord.Captcha.Rendering;

public interface IGhostFontRenderer
{
    byte[] RenderGif(string code, int seed);
    byte[] RenderWav(string code, int seed);
}
