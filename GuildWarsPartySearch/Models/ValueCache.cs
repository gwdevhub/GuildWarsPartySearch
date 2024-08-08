using System.Core.Extensions;

namespace GuildWarsPartySearch.Server.Models;

public sealed class ValueCache<T>
    where T : class
{
    private readonly Func<Task<T>> cacheRefreshOperation;
    private readonly TimeSpan cacheDuration;

    private T? value;
    private DateTime lastCacheRefresh = DateTime.MinValue;

    public ValueCache(Func<Task<T>> cacheRefreshOperation, TimeSpan cacheDuration)
    {
        this.cacheRefreshOperation = cacheRefreshOperation.ThrowIfNull();
        this.cacheDuration = cacheDuration;
    }

    public async Task<T> GetValue()
    {
        if (DateTime.Now - this.lastCacheRefresh < this.cacheDuration &&
            this.value is not null)
        {
            return this.value;
        }

        return await this.PerformCacheRefresh();
    }

    public async Task<T> ForceCacheRefresh()
    {
        return await this.PerformCacheRefresh();
    }

    private async Task<T> PerformCacheRefresh()
    {
        this.value = await this.cacheRefreshOperation();
        this.lastCacheRefresh = DateTime.Now;
        return this.value;
    }
}
