﻿using Azure;
using Azure.Data.Tables;

namespace GuildWarsPartySearch.Server.Services.Database.Models;

public sealed class PartySearchTableEntity : ITableEntity
{
    public string PartitionKey { get; set; } = default!;

    public string RowKey { get; set; } = default!;

    public int DistrictRegion { get; set; }

    public int DistrictNumber { get; set; }

    public int DistrictLanguage { get; set; }

    public int MapId { get; set; }

    public int PartyId { get; set; }

    public string? Message { get; set; }

    public string? Sender { get; set; }

    public int PartySize { get; set; }

    public int HeroCount { get; set; }

    public int HardMode { get; set; }

    public int SearchType { get; set; }

    public int Primary { get; set; }

    public int Secondary { get; set; }

    public int Level { get; set; }

    public DateTimeOffset? Timestamp { get; set; }

    public ETag ETag { get; set; }
}
