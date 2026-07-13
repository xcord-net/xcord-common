using System.Collections.Concurrent;

namespace Xcord.Captcha.Storage;

/// <summary>
/// Zero-dependency in-memory captcha store. Ships as the default so the module runs standalone
/// in any project (and in tests/dev without Redis).
/// </summary>
public sealed class InMemoryCaptchaStore : ICaptchaStore
{
    private sealed record Entry(string Answer, int Seed, DateTimeOffset ExpiresAt);

    private readonly ConcurrentDictionary<string, Entry> _map = new();

    public Task SaveAsync(string id, string answer, int seed, TimeSpan ttl)
    {
        Sweep();
        _map[id] = new Entry(answer, seed, DateTimeOffset.UtcNow + ttl);
        return Task.CompletedTask;
    }

    public Task<int?> PeekSeedAsync(string id)
        => Task.FromResult(TryGet(id, out var e) ? e!.Seed : (int?)null);

    public Task<string?> PeekAnswerAsync(string id)
        => Task.FromResult(TryGet(id, out var e) ? e!.Answer : null);

    public Task<string?> TakeAnswerAsync(string id)
    {
        if (TryGet(id, out var e) && _map.TryRemove(id, out _))
            return Task.FromResult<string?>(e!.Answer);
        return Task.FromResult<string?>(null);
    }

    private bool TryGet(string id, out Entry? entry)
    {
        if (_map.TryGetValue(id, out var e) && e.ExpiresAt > DateTimeOffset.UtcNow)
        {
            entry = e;
            return true;
        }
        _map.TryRemove(id, out _);
        entry = null;
        return false;
    }

    private void Sweep()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var kv in _map)
            if (kv.Value.ExpiresAt <= now)
                _map.TryRemove(kv.Key, out _);
    }
}
