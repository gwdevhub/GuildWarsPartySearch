using Azure;
using Azure.Data.Tables;

namespace GuildWarsPartySearch.Server.Services.Database.Models;

public sealed class PartySearchTableEntity : ITableEntity
{
    public string PartitionKey { get; set; } = default!;

    public string RowKey { get; set; } = default!;

    public string? Campaign { get; set; }

    public string? Continent { get; set; }

    public string? Region { get; set; }

    public string? Map { get; set; }

    public string? District { get; set; }

    public string? CharName { get; set; }

    public int? PartySize { get; set; }

    public int? PartyMaxSize { get; set; }

    public int? Npcs { get; set; }

    public DateTimeOffset? Timestamp { get; set; }

    public ETag ETag { get; set; }
}
