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
using GuildWarsPartySearch.Server.Services.Processing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Settings.Configuration;
using Serilog.Sinks.File;
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
            .SetBasePath(GetBasePath())
            .AddJsonFile("Config.json", false)
            .AddEnvironmentVariables();

        return builder;
    }

    public static ILoggingBuilder SetupLogging(this ILoggingBuilder builder, IConfigurationRoot config)
    {
        builder.ThrowIfNull()
            .ClearProviders()
            .AddSerilog(logger: new LoggerConfiguration()
                .ReadFrom.Configuration(config, readerOptions: new ConfigurationReaderOptions
                {
                    SectionName = "Logging"
                })
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] {Level:u4}: [{ClientIP}] [{ApiKey}] [{CorrelationVector}] [{SourceContext}]{NewLine}{Message:lj}{NewLine}{Exception}",
                    theme: Serilog.Sinks.SystemConsole.Themes.SystemConsoleTheme.Colored)
                .WriteTo.File(
                    path: Path.Combine(GetBasePath(), "logs/log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    fileSizeLimitBytes: 10 * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] {Level:u4}: [{ClientIP}] [{ApiKey}] [{CorrelationVector}] [{SourceContext}] - {Message:lj}{NewLine}{Exception}")
                .CreateLogger());

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
            .ConfigureExtended<ApiWhitelistOptions>()
            .ConfigureExtended<TextProcessorOptions>();
    }

    public static IServiceCollection SetupServices(this IServiceCollection services)
    {
        services.ThrowIfNull();
        services.AddSingleton<ILiveFeedService, LiveFeedService>();
        services.AddSingleton<IBotStatusService, BotStatusService>();
        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<SQLiteDatabaseOptions>>();
            var absolutePath = Path.Combine(GetBasePath(), options.Value.Path);
            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
            var connection = new SqliteConnection($"Data Source={absolutePath}");
            connection.Open();
            return connection;
        });
        services.AddSingleton<IPartySearchDatabase, PartySearchSqliteDatabase>();
        services.AddSingleton<IBotHistoryDatabase, BotHistorySqliteDatabase>();
        services.AddSingleton<IApiKeyDatabase, ApiKeySqliteDatabase>();
        services.AddSingleton<ITextProcessor, TextProcessor>();
        services.AddScoped<IPExtractingMiddleware>();
        services.AddScoped<PermissioningMiddleware>();
        services.AddScoped<HeaderLoggingMiddleware>();
        services.AddScoped<CorrelationVectorMiddleware>();
        services.AddScoped<UserAgentRequired>();
        services.AddScoped<AdminPermissionRequired>();
        services.AddScoped<BotPermissionRequired>();
        services.AddScoped<LoggingEnrichmentMiddleware>();
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

    private static string GetBasePath()
    {
        return Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location) ?? throw new InvalidOperationException("Unable to get base path");
    }
}
