using StackExchange.Redis;

namespace Xcord.Captcha.Storage;

/// <summary>
/// Redis-backed captcha store (optional adapter). Delete this file if the consuming project does
/// not use Redis; the in-memory store covers standalone use. Each challenge is stored as a hash
/// (answer + seed) under a configurable, namespaced key.
/// </summary>
public sealed class RedisCaptchaStore(IConnectionMultiplexer redis, string keyPrefix = "captcha:") : ICaptchaStore
{
    private const string AnswerField = "a";
    private const string SeedField = "s";

    private IDatabase Db => redis.GetDatabase();
    private string Key(string id) => $"{keyPrefix}{id}";

    public async Task SaveAsync(string id, string answer, int seed, TimeSpan ttl)
    {
        var key = Key(id);
        await Db.HashSetAsync(key, new HashEntry[] { new(AnswerField, answer), new(SeedField, seed) })
            .ConfigureAwait(false);
        await Db.KeyExpireAsync(key, ttl).ConfigureAwait(false);
    }

    public async Task<int?> PeekSeedAsync(string id)
    {
        var v = await Db.HashGetAsync(Key(id), SeedField).ConfigureAwait(false);
        return v.IsNull ? null : (int)v;
    }

    public async Task<string?> PeekAnswerAsync(string id)
    {
        var v = await Db.HashGetAsync(Key(id), AnswerField).ConfigureAwait(false);
        return v.IsNull ? null : v.ToString();
    }

    public async Task<string?> TakeAnswerAsync(string id)
    {
        var key = Key(id);
        // Read the answer and delete the whole entry in one atomic transaction (single-use).
        var tran = Db.CreateTransaction();
        var answerTask = tran.HashGetAsync(key, AnswerField);
        _ = tran.KeyDeleteAsync(key);
        if (!await tran.ExecuteAsync().ConfigureAwait(false)) return null;
        var answer = await answerTask.ConfigureAwait(false);
        return answer.IsNull ? null : answer.ToString();
    }
}
