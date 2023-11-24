using GuildWarsPartySearch.Common.Models.GuildWars;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace GuildWarsPartySearch.Common.Converters;

public sealed class MapTypeConverter : TypeConverter
{
    public override bool CanConvertTo(ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType)
    {
        return destinationType == typeof(Map) ||
            destinationType == typeof(string) ||
            destinationType == typeof(int);
    }

    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(Map) ||
            sourceType == typeof(string) ||
            sourceType == typeof(int);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string) && value is Map stringMap)
        {
            return stringMap.Name;
        }
        else if (destinationType == typeof(int) && value is Map intMap)
        {
            return intMap.Id;
        }
        else if (destinationType == typeof(Map) && value is string s)
        {
            return Map.Parse(s);
        }
        else if (destinationType == typeof(Map) && value is int id)
        {
            return Map.Parse(id);
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string s)
        {
            return Map.Parse(s);
        }
        else if (value is int id)
        {
            Map.Parse(id);
        }
        else if (value is Map map)
        {
            return map.Name;
        }

        return base.ConvertFrom(context, culture, value);
    }
}
