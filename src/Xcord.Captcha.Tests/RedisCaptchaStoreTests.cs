using StackExchange.Redis;
using Testcontainers.Redis;
using Xcord.Captcha.Storage;
using Xunit;

public class RedisCaptchaStoreTests : IAsyncLifetime
{
    private readonly RedisContainer _redis = new RedisBuilder().WithImage("redis:7-alpine").Build();
    private IConnectionMultiplexer _mux = null!;

    public async Task InitializeAsync()
    {
        await _redis.StartAsync();
        _mux = await ConnectionMultiplexer.ConnectAsync(_redis.GetConnectionString());
    }

    public async Task DisposeAsync()
    {
        await _mux.DisposeAsync();
        await _redis.DisposeAsync();
    }

    [Fact]
    public async Task Seed_and_answer_roundtrip_with_single_use()
    {
        var store = new RedisCaptchaStore(_mux, "test:captcha:");
        await store.SaveAsync("id1", "ACDEF", 77, TimeSpan.FromMinutes(5));

        Assert.Equal(77, await store.PeekSeedAsync("id1"));
        Assert.Equal("ACDEF", await store.PeekAnswerAsync("id1"));

        Assert.Equal("ACDEF", await store.TakeAnswerAsync("id1"));
        Assert.Null(await store.TakeAnswerAsync("id1")); // consumed
        Assert.Null(await store.PeekSeedAsync("id1"));
    }

    [Fact]
    public async Task Entry_expires_after_ttl()
    {
        var store = new RedisCaptchaStore(_mux, "test:captcha:");
        await store.SaveAsync("id2", "HJKMN", 1, TimeSpan.FromMilliseconds(200));
        await Task.Delay(400);
        Assert.Null(await store.PeekSeedAsync("id2"));
    }
}
