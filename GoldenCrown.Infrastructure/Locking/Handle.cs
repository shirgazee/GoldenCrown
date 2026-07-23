using StackExchange.Redis;

namespace GoldenCrown.Infrastructure.Locking;

public class Handle : IDistributedLockHandle
{
    private readonly IDatabase _db;
    private readonly string _token; // guid
    
    public string Key { get; } // goldencrown:currencies:USD

    public Handle(IDatabase db, string key, string token)
    {
        _db = db;
        Key = key;
        _token = token;
    }

    public async ValueTask DisposeAsync()
    {
        await _db.LockReleaseAsync(Key, _token);
    }
}