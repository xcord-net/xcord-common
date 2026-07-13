using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xcord.Captcha.AspNetCore;
using Xunit;

public class CaptchaEndpointsTests
{
    private static IHost BuildHost()
        => new HostBuilder()
            .ConfigureWebHost(web =>
            {
                web.UseTestServer();
                web.ConfigureServices(s =>
                {
                    s.AddRouting();
                    s.AddGhostFontCaptcha();
                });
                web.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(e => e.MapGhostFontCaptcha());
                });
            })
            .Start();

    [Fact]
    public async Task Get_captcha_returns_ids_and_urls()
    {
        using var host = BuildHost();
        var client = host.GetTestClient();
        var meta = await client.GetFromJsonAsync<CaptchaApiResponse>("/api/v1/auth/captcha");
        Assert.NotNull(meta);
        Assert.False(string.IsNullOrEmpty(meta!.CaptchaId));
        Assert.EndsWith(".gif", meta.ImageUrl);
        Assert.EndsWith(".wav", meta.AudioUrl);
    }

    [Fact]
    public async Task Get_gif_for_known_id_returns_image()
    {
        using var host = BuildHost();
        var client = host.GetTestClient();
        var meta = await client.GetFromJsonAsync<CaptchaApiResponse>("/api/v1/auth/captcha");
        var resp = await client.GetAsync(meta!.ImageUrl);
        resp.EnsureSuccessStatusCode();
        Assert.Equal("image/gif", resp.Content.Headers.ContentType!.MediaType);
    }

    [Fact]
    public async Task Get_wav_for_known_id_returns_audio()
    {
        using var host = BuildHost();
        var client = host.GetTestClient();
        var meta = await client.GetFromJsonAsync<CaptchaApiResponse>("/api/v1/auth/captcha");
        var resp = await client.GetAsync(meta!.AudioUrl);
        resp.EnsureSuccessStatusCode();
        Assert.Equal("audio/wav", resp.Content.Headers.ContentType!.MediaType);
    }

    [Fact]
    public async Task Get_gif_for_unknown_id_returns_404()
    {
        using var host = BuildHost();
        var client = host.GetTestClient();
        var resp = await client.GetAsync("/api/v1/auth/captcha/deadbeef.gif");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }
}
