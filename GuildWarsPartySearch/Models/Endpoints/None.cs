using GuildWarsPartySearch.Server.Converters;
using System.ComponentModel;

namespace GuildWarsPartySearch.Server.Models.Endpoints;

[TypeConverter(typeof(NoneConverter))]
public sealed class None
{
}
