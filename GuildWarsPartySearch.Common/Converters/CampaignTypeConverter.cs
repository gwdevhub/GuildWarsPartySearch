using GuildWarsPartySearch.Common.Models.GuildWars;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace GuildWarsPartySearch.Common.Converters;

public sealed class CampaignTypeConverter : TypeConverter
{
    public override bool CanConvertTo(ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType)
    {
        return destinationType == typeof(Campaign) ||
            destinationType == typeof(string) ||
            destinationType == typeof(int);
    }

    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(Campaign) ||
            sourceType == typeof(string) ||
            sourceType == typeof(int);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string) && value is Campaign stringCampaign)
        {
            return stringCampaign.Name;
        }
        else if(destinationType == typeof(int) && value is Campaign intCampaign)
        {
            return intCampaign.Id;
        }
        else if (destinationType == typeof(Campaign) && value is string s)
        {
            return Campaign.Parse(s);
        }
        else if (destinationType == typeof(Campaign) && value is int id)
        {
            return Campaign.Parse(id);
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string s)
        {
            return Campaign.Parse(s);
        }
        else if (value is int id)
        {
            Campaign.Parse(id);
        }
        else if (value is Campaign campaign)
        {
            return campaign.Name;
        }

        return base.ConvertFrom(context, culture, value);
    }
}
