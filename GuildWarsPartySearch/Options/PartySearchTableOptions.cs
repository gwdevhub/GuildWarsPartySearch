﻿using GuildWarsPartySearch.Server.Options.Azure;

namespace GuildWarsPartySearch.Server.Options;

public class PartySearchTableOptions : IAzureTableStorageOptions
{
    public string TableName { get; set; } = default!;
}
