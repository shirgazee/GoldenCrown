using StackExchange.Redis;

namespace GoldenCrown.Infrastructure.Locking;

public class RedisDistributedLock : IDistributedLock
{
    private readonly IDatabase _db;

    public RedisDistributedLock(IConnectionMultiplexer connectionMultiplexer)
    {
        _db = connectionMultiplexer.GetDatabase();
    }
    
    public async Task<IDistributedLockHandle?> TryAcquireLockAsync(string key, TimeSpan ttl, CancellationToken ct)
    {
        string token = Guid.NewGuid().ToString();
        bool ok = await _db.LockTakeAsync(key, token, ttl);
        
        return ok ? new Handle(_db, key, token) : null;
    }
}