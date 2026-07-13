using Xcord.Captcha.Storage;
using Xunit;

public class InMemoryCaptchaStoreTests
{
    [Fact]
    public async Task Save_then_peek_returns_seed_and_answer_and_keeps_entry()
    {
        var s = new InMemoryCaptchaStore();
        await s.SaveAsync("id1", "ACDEF", 42, TimeSpan.FromMinutes(5));
        Assert.Equal(42, await s.PeekSeedAsync("id1"));
        Assert.Equal("ACDEF", await s.PeekAnswerAsync("id1"));
        Assert.Equal(42, await s.PeekSeedAsync("id1")); // still there after peeks
        Assert.Equal("ACDEF", await s.PeekAnswerAsync("id1"));
    }

    [Fact]
    public async Task TakeAnswer_is_single_use()
    {
        var s = new InMemoryCaptchaStore();
        await s.SaveAsync("id1", "ACDEF", 1, TimeSpan.FromMinutes(5));
        Assert.Equal("ACDEF", await s.TakeAnswerAsync("id1"));
        Assert.Null(await s.TakeAnswerAsync("id1"));
        Assert.Null(await s.PeekSeedAsync("id1"));
    }

    [Fact]
    public async Task Expired_entry_is_gone()
    {
        var s = new InMemoryCaptchaStore();
        await s.SaveAsync("id1", "ACDEF", 1, TimeSpan.FromMilliseconds(1));
        await Task.Delay(20);
        Assert.Null(await s.PeekSeedAsync("id1"));
        Assert.Null(await s.PeekAnswerAsync("id1"));
        Assert.Null(await s.TakeAnswerAsync("id1"));
    }

    [Fact]
    public async Task Unknown_id_returns_null()
    {
        var s = new InMemoryCaptchaStore();
        Assert.Null(await s.PeekSeedAsync("nope"));
        Assert.Null(await s.PeekAnswerAsync("nope"));
        Assert.Null(await s.TakeAnswerAsync("nope"));
    }
}
