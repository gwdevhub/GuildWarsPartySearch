using GuildWarsPartySearch.Server.Attributes;
using System.Core.Extensions;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureExtended<TOptions>(this WebApplicationBuilder builder)
        where TOptions : class, new()
    {
        builder.ThrowIfNull()
               .Services.Configure<TOptions>(builder.Configuration.GetSection(GetOptionsName<TOptions>()));
        return builder;
    }

    public static IConfigurationSection GetRequiredSection<TOptions>(this ConfigurationManager configurationManager)
    {
        configurationManager.ThrowIfNull();
        return configurationManager.GetRequiredSection(GetOptionsName<TOptions>());
    }

    private static string GetOptionsName<TOptions>()
    {
        var maybeAttribute = typeof(TOptions).GetCustomAttributes(false).OfType<OptionsNameAttribute>().FirstOrDefault();
        if (maybeAttribute is not null &&
            maybeAttribute.Name?.IsNullOrWhiteSpace() is false)
        {
            return maybeAttribute.Name;
        }

        return typeof(TOptions).Name;
    }
}
