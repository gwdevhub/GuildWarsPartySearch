namespace GuildWarsPartySearch.Server.Services.Processing;

public interface ITextProcessor
{
    bool IsSpam(string? text);
}
