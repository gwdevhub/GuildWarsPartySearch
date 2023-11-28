using Azure.Core;
using Azure.Identity;
using GuildWarsPartySearch.Server.Attributes;
using GuildWarsPartySearch.Server.Options.Azure;
using Microsoft.Extensions.Options;
using System.Core.Extensions;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureAzureClientSecretCredentials<TOptions>(this WebApplicationBuilder builder)
        where TOptions : class, IAzureClientSecretCredentialOptions, new()
    {
        builder.ThrowIfNull()
            .ConfigureExtended<TOptions>()
            .Services.AddSingleton<TokenCredential>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<TOptions>>().Value;
                return new ClientSecretCredential(options.TenantId, options.ClientId, options.ClientSecret);
            });

        return builder;
    }

    public static WebApplicationBuilder ConfigureExtended<TOptions>(this WebApplicationBuilder builder)
        where TOptions : class, new()
    {
        builder.ThrowIfNull()
               .Services.Configure<TOptions>(builder.Configuration.GetSection(GetOptionsName<TOptions>()));
        return builder;
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
