using GuildWarsPartySearch.Common.Models.GuildWars;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace GuildWarsPartySearch.Common.Converters;

public sealed class RegionTypeConverter : TypeConverter
{
    public override bool CanConvertTo(ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType)
    {
        return destinationType == typeof(Region) ||
            destinationType == typeof(string) ||
            destinationType == typeof(int);
    }

    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(Region) ||
            sourceType == typeof(string) ||
            sourceType == typeof(int);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string) && value is Region stringRegion)
        {
            return stringRegion.Name;
        }
        else if (destinationType == typeof(int) && value is Region intRegion)
        {
            return intRegion.Id;
        }
        else if (destinationType == typeof(Region) && value is string s)
        {
            return Region.Parse(s);
        }
        else if (destinationType == typeof(Region) && value is int id)
        {
            return Region.Parse(id);
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string s)
        {
            return Region.Parse(s);
        }
        else if (value is int id)
        {
            Region.Parse(id);
        }
        else if (value is Region region)
        {
            return region.Name;
        }

        return base.ConvertFrom(context, culture, value);
    }
}
