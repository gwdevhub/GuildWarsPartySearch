using Azure;

namespace GuildWarsPartySearch.Server.Services.Azure;

public sealed class InterceptingPageable<T> : Pageable<T> where T: notnull
{
    private readonly Action<Page<T>> interceptPage;
    private readonly Action interceptSuccess;
    private readonly Pageable<T> originalPageable;

    public InterceptingPageable(Pageable<T> originalPageable, Action<Page<T>> interceptPage, Action interceptSuccess)
    {
        this.originalPageable = originalPageable;
        this.interceptPage = interceptPage;
        this.interceptSuccess = interceptSuccess;
    }

    public override IEnumerable<Page<T>> AsPages(string? continuationToken = null, int? pageSizeHint = null)
    {
        foreach (var page in this.originalPageable.AsPages(continuationToken, pageSizeHint))
        {
            this.interceptPage(page);
            yield return page;
        }
    }

    public override IEnumerator<T> GetEnumerator()
    {
        foreach (var item in this.originalPageable)
        {
            this.interceptSuccess();
            yield return item;
        }
    }
}
