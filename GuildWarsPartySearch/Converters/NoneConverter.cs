using GuildWarsPartySearch.Server.Models.Endpoints;
using MTSC.Common.Http;
using System.ComponentModel;
using System.Globalization;

namespace GuildWarsPartySearch.Server.Converters;

public sealed class NoneConverter : TypeConverter
{
    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object? value)
    {
        if (value is HttpRequestContext)
        {
            return new None();
        }

        return base.ConvertFrom(context, culture, value!)!;
    }
}
