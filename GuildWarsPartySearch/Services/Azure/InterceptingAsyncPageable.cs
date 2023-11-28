using Azure;

namespace GuildWarsPartySearch.Server.Services.Azure;

public class InterceptingAsyncPageable<T> : AsyncPageable<T> where T : notnull
{
    private readonly Action<Page<T>> interceptPage;
    private readonly Action interceptSuccess;
    private readonly AsyncPageable<T> originalPageable;

    public InterceptingAsyncPageable(AsyncPageable<T> originalPageable, Action<Page<T>> interceptPage, Action interceptSuccess)
    {
        this.originalPageable = originalPageable;
        this.interceptPage = interceptPage;
        this.interceptSuccess = interceptSuccess;
    }

    public override async IAsyncEnumerable<Page<T>> AsPages(string? continuationToken = null, int? pageSizeHint = null)
    {
        await foreach (var page in this.originalPageable.AsPages(continuationToken, pageSizeHint))
        {
            this.interceptPage(page);
            yield return page;
        }
    }

    public async override IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        await foreach(var item in this.originalPageable)
        {
            this.interceptSuccess();
            yield return item;
        }
    }
}
