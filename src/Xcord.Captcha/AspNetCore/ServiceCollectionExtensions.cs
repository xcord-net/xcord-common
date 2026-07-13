using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Xcord.Captcha.Rendering;
using Xcord.Captcha.Storage;

namespace Xcord.Captcha.AspNetCore;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the ghost-font captcha renderer, service, and (by default) the in-memory store.
    /// Call <see cref="UseRedisCaptchaStore"/> afterwards to switch to Redis.
    /// </summary>
    public static IServiceCollection AddGhostFontCaptcha(
        this IServiceCollection services, Action<CaptchaOptions>? configure = null)
    {
        var opts = new CaptchaOptions();
        configure?.Invoke(opts);

        services.TryAddSingleton(Options.Create(opts));
        services.TryAddSingleton<IGhostFontRenderer, GhostFontRenderer>();
        services.TryAddSingleton<ICaptchaStore, InMemoryCaptchaStore>();

        services.RemoveAll<ICaptchaService>();
        if (opts.Enabled)
            services.AddScoped<ICaptchaService, CaptchaService>();
        else
            services.AddSingleton<ICaptchaService, NoOpCaptchaService>();

        return services;
    }

    /// <summary>
    /// Replaces the default in-memory store with a Redis-backed store. Requires an
    /// <see cref="IConnectionMultiplexer"/> to already be registered.
    /// </summary>
    public static IServiceCollection UseRedisCaptchaStore(this IServiceCollection services)
    {
        services.RemoveAll<ICaptchaStore>();
        services.AddSingleton<ICaptchaStore>(sp =>
            new RedisCaptchaStore(
                sp.GetRequiredService<IConnectionMultiplexer>(),
                sp.GetRequiredService<IOptions<CaptchaOptions>>().Value.KeyPrefix));
        return services;
    }
}
