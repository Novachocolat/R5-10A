using StackExchange.Redis;

namespace PixelHub.Api.Repositories;

// Présence en ligne : sorted set dont le score est l'horodatage du dernier signe de vie.
public class RedisPresence
{
    private const string Key = "enligne";
    private static readonly TimeSpan Window = TimeSpan.FromSeconds(60);
    private readonly IDatabase _db;

    public RedisPresence(IConnectionMultiplexer redis) => _db = redis.GetDatabase();

    private static double Now() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    // ZADD : crée le membre ou met à jour son score (dernier signe de vie).
    public async Task TouchAsync(string pseudo) =>
        await _db.SortedSetAddAsync(Key, pseudo, Now());

    public async Task<long> CountOnlineAsync()
    {
        var limite = Now() - Window.TotalSeconds;
        // Purge : ZREMRANGEBYSCORE -inf (limite
        await _db.SortedSetRemoveRangeByScoreAsync(Key, double.NegativeInfinity, limite, Exclude.Stop);
        // Comptage : ZCARD
        return await _db.SortedSetLengthAsync(Key);
    }
}
