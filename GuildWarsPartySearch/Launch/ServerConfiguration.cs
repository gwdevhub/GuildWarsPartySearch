using GuildWarsPartySearch.Common.Converters;
using GuildWarsPartySearch.Server.Endpoints;
using GuildWarsPartySearch.Server.Extensions;
using GuildWarsPartySearch.Server.Filters;
using GuildWarsPartySearch.Server.Middleware;
using GuildWarsPartySearch.Server.Options;
using GuildWarsPartySearch.Server.Services.BotStatus;
using GuildWarsPartySearch.Server.Services.CharName;
using GuildWarsPartySearch.Server.Services.Database;
using GuildWarsPartySearch.Server.Services.Feed;
using GuildWarsPartySearch.Server.Services.PartySearch;
using GuildWarsPartySearch.Server.Services.Permissions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using System.Core.Extensions;
using System.Extensions;
using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Server.Launch;

public static class ServerConfiguration
{
    public static IList<JsonConverter> SetupConverters(this IList<JsonConverter> converters)
    {
        converters.ThrowIfNull();
        converters.Add(new MapJsonConverter());
        converters.Add(new ProfessionJsonConverter());

        return converters;
    }

    public static WebApplicationBuilder SetupHostedServices(this WebApplicationBuilder builder)
    {
        return builder;
    }

    public static IConfigurationBuilder SetupConfiguration(this IConfigurationBuilder builder)
    {
        builder.ThrowIfNull()
            .SetBasePath(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location) ?? throw new InvalidOperationException("Unable to figure out base directory"))
            .AddJsonFile("Config.json", false)
            .AddEnvironmentVariables();

        return builder;
    }

    public static ILoggingBuilder SetupLogging(this ILoggingBuilder builder)
    {
        builder.ThrowIfNull()
            .ClearProviders()
            .AddConsole(o => o.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ");

        return builder;
    }

    public static WebApplicationBuilder SetupOptions(this WebApplicationBuilder builder)
    {
        return builder.ThrowIfNull()
            .ConfigureExtended<EnvironmentOptions>()
            .ConfigureExtended<ContentOptions>()
            .ConfigureExtended<PartySearchDatabaseOptions>()
            .ConfigureExtended<ServerOptions>()
            .ConfigureExtended<IpWhitelistOptions>()
            .ConfigureExtended<BotHistoryDatabaseOptions>()
            .ConfigureExtended<SQLiteDatabaseOptions>()
            .ConfigureExtended<ApiWhitelistOptions>();
    }

    public static IServiceCollection SetupServices(this IServiceCollection services)
    {
        services.ThrowIfNull();
        services.AddSingleton<ILiveFeedService, LiveFeedService>();
        services.AddSingleton<IBotStatusService, BotStatusService>();
        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<SQLiteDatabaseOptions>>();
            var absolutePath = Path.Combine(
                Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location) ?? throw new InvalidOperationException("Unable to figure out base directory"), options.Value.Path);
            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
            var connection = new SqliteConnection($"Data Source={absolutePath}");
            connection.Open();
            return connection;
        });
        services.AddSingleton<IPartySearchDatabase, PartySearchSqliteDatabase>();
        services.AddSingleton<IBotHistoryDatabase, BotHistorySqliteDatabase>();
        services.AddSingleton<IApiKeyDatabase, ApiKeySqliteDatabase>();
        services.AddScoped<IPExtractingMiddleware>();
        services.AddScoped<PermissioningMiddleware>();
        services.AddScoped<UserAgentRequired>();
        services.AddScoped<AdminPermissionRequired>();
        services.AddScoped<BotPermissionRequired>();
        services.AddScoped<IPartySearchService, PartySearchService>();
        services.AddScoped<ICharNameValidator, CharNameValidator>();
        services.AddScoped<IPermissionService, PermissionService>();
        return services;
    }

    public static WebApplication SetupRoutes(this WebApplication app)
    {
        app.ThrowIfNull()
           .MapWebSocket<LiveFeed>("party-search/live-feed")
           .MapWebSocket<PostPartySearch>("party-search/update");

        return app;
    }
}
