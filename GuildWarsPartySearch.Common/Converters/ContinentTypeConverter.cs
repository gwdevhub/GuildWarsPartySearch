using GuildWarsPartySearch.Common.Models.GuildWars;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace GuildWarsPartySearch.Common.Converters;

public sealed class ContinentTypeConverter : TypeConverter
{
    public override bool CanConvertTo(ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType)
    {
        return destinationType == typeof(Continent) ||
            destinationType == typeof(string) ||
            destinationType == typeof(int);
    }

    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(Continent) ||
            sourceType == typeof(string) ||
            sourceType == typeof(int);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string) && value is Continent stringContinent)
        {
            return stringContinent.Name;
        }
        else if (destinationType == typeof(int) && value is Continent intContinent)
        {
            return intContinent.Id;
        }
        else if (destinationType == typeof(Continent) && value is string s)
        {
            return Continent.Parse(s);
        }
        else if (destinationType == typeof(Continent) && value is int id)
        {
            return Continent.Parse(id);
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string s)
        {
            return Continent.Parse(s);
        }
        else if (value is int id)
        {
            Continent.Parse(id);
        }
        else if (value is Continent continent)
        {
            return continent.Name;
        }

        return base.ConvertFrom(context, culture, value);
    }
}
