using GuildWarsPartySearch.Common.Models.GuildWars;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace GuildWarsPartySearch.Common.Converters;

public sealed class ProfessionTypeConverter : TypeConverter
{
    public override bool CanConvertTo(ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType)
    {
        return destinationType == typeof(Profession) ||
            destinationType == typeof(string) ||
            destinationType == typeof(int);
    }

    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(Profession) ||
            sourceType == typeof(string) ||
            sourceType == typeof(int);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string) && value is Profession stringProfession)
        {
            return stringProfession.Name;
        }
        else if (destinationType == typeof(int) && value is Profession intProfession)
        {
            return intProfession.Id;
        }
        else if (destinationType == typeof(Profession) && value is string s)
        {
            return Profession.Parse(s);
        }
        else if (destinationType == typeof(Profession) && value is int id)
        {
            return Profession.Parse(id);
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string s)
        {
            return Profession.Parse(s);
        }
        else if (value is int id)
        {
            Profession.Parse(id);
        }
        else if (value is Profession profession)
        {
            return profession.Name;
        }

        return base.ConvertFrom(context, culture, value);
    }
}

