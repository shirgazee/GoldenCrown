using System.Reflection.Metadata;

namespace GoldenCrown.Infrastructure.Locking;

public interface IDistributedLock
{
    Task<IDistributedLockHandle?> TryAcquireLockAsync(string key, TimeSpan ttl, CancellationToken ct);
}

public interface IDistributedLockHandle : IAsyncDisposable
{
    string Key { get; }
}