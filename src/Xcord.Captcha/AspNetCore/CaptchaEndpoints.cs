using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Xcord.Captcha.AspNetCore;

public sealed record CaptchaApiResponse(string CaptchaId, string ImageUrl, string AudioUrl);

public static class CaptchaEndpoints
{
    /// <summary>
    /// Maps the 3 captcha routes under <paramref name="basePath"/>:
    /// GET {base} -> ids + urls, GET {base}/{id}.gif -> image, GET {base}/{id}.wav -> audio.
    /// </summary>
    public static RouteGroupBuilder MapGhostFontCaptcha(
        this IEndpointRouteBuilder app, string basePath = "/api/v1/auth/captcha")
    {
        var group = app.MapGroup(basePath).WithTags("Auth").AllowAnonymous();

        group.MapGet("", async (ICaptchaService svc) =>
        {
            var challenge = await svc.GenerateAsync().ConfigureAwait(false);
            return Results.Ok(new CaptchaApiResponse(
                challenge.Id, $"{basePath}/{challenge.Id}.gif", $"{basePath}/{challenge.Id}.wav"));
        });

        group.MapGet("/{id}.gif", async (string id, ICaptchaService svc) =>
        {
            var bytes = await svc.RenderGifAsync(id).ConfigureAwait(false);
            return bytes is null ? Results.NotFound() : Results.File(bytes, "image/gif");
        });

        group.MapGet("/{id}.wav", async (string id, ICaptchaService svc) =>
        {
            var bytes = await svc.RenderWavAsync(id).ConfigureAwait(false);
            return bytes is null ? Results.NotFound() : Results.File(bytes, "audio/wav");
        });

        return group;
    }
}
