using Azure.Storage.Blobs.Models;
using GuildWarsPartySearch.Server.Options;
using System.Extensions;
using Microsoft.Extensions.Options;
using System.Core.Extensions;
using GuildWarsPartySearch.Server.Services.Azure;

namespace GuildWarsPartySearch.Server.Services.Content;

public sealed class ContentRetrievalService : BackgroundService
{
    private readonly Dictionary<string, DateTime> fileMetadatas = [];

    private readonly NamedBlobContainerClient<ContentOptions> namedBlobContainerClient;
    private readonly EnvironmentOptions environmentOptions;
    private readonly ContentOptions contentOptions;
    private readonly ILogger<ContentRetrievalService> logger;

    public ContentRetrievalService(
        NamedBlobContainerClient<ContentOptions> namedBlobContainerClient,
        IOptions<EnvironmentOptions> environmentOptions,
        IOptions<ContentOptions> contentOptions,
        ILogger<ContentRetrievalService> logger)
    {
        this.namedBlobContainerClient = namedBlobContainerClient.ThrowIfNull();
        this.environmentOptions = environmentOptions.Value.ThrowIfNull();
        this.contentOptions = contentOptions.Value.ThrowIfNull();
        this.logger = logger.ThrowIfNull();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var scopedLogger = logger.CreateScopedLogger(nameof(this.ExecuteAsync), string.Empty);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateContent(stoppingToken);
            }
            catch (Exception ex)
            {
                scopedLogger.LogError(ex, "Encountered exception while retrieving content");
            }

            await Task.Delay(contentOptions.UpdateFrequency, stoppingToken);
        }
    }

    private async Task UpdateContent(CancellationToken cancellationToken)
    {
        var scopedLogger = logger.CreateScopedLogger(nameof(this.UpdateContent), string.Empty);
        if (this.environmentOptions.Name == "Local")
        {
            scopedLogger.LogDebug("Content retrieval is disabled in local");
            return;
        }

        scopedLogger.LogDebug("Checking content to retrieve");
        var blobs = this.namedBlobContainerClient.GetBlobsAsync(cancellationToken: cancellationToken);

        if (!Directory.Exists(contentOptions.StagingFolder))
        {
            Directory.CreateDirectory(contentOptions.StagingFolder);
        }

        scopedLogger.LogDebug($"Retrieving blobs");
        var blobList = new List<BlobItem>();
        await foreach (var blob in blobs)
        {
            blobList.Add(blob);
        }

        var stagingFolderFullPath = Path.GetFullPath(contentOptions.StagingFolder);
        var stagedFiles = Directory.GetFiles(contentOptions.StagingFolder, "*", SearchOption.AllDirectories);
        var filesToDelete = stagedFiles
            .Select(f => Path.GetFullPath(f).Replace(stagingFolderFullPath, "").Replace('\\', '/').Trim('/'))
            .Where(f => blobList.None(b => b.Name == f));
        foreach (var file in filesToDelete)
        {
            scopedLogger.LogDebug($"[{file}] File not in blob. Deleting");
            File.Delete($"{stagingFolderFullPath}\\{file}");
            fileMetadatas.Remove(file);

        }

        foreach (var blob in blobList)
        {
            var finalPath = Path.Combine(contentOptions.StagingFolder, blob.Name);
            var fileInfo = new FileInfo(finalPath);
            fileInfo.Directory!.Create();
            if (fileInfo.Exists &&
                fileInfo.Length == blob.Properties.ContentLength &&
                fileMetadatas.TryGetValue(blob.Name, out var lastChangeDate) &&
                lastChangeDate == blob.Properties.LastModified?.UtcDateTime)
            {
                scopedLogger.LogDebug($"[{blob.Name}] File unchanged. Skipping");
                continue;
            }

            var blobClient = this.namedBlobContainerClient.GetBlobClient(blob.Name);
            using var fileStream = new FileStream(finalPath, FileMode.Create);
            using var blobStream = await blobClient.OpenReadAsync(new BlobOpenReadOptions(false)
            {
                BufferSize = 1024,
            }, cancellationToken);

            scopedLogger.LogDebug($"[{blob.Name}] Downloading blob");
            await blobStream.CopyToAsync(fileStream, cancellationToken);
            scopedLogger.LogInformation($"[{blob.Name}] Downloaded blob");

            fileInfo = new FileInfo(finalPath);
            fileMetadatas[blob.Name] = blob.Properties.LastModified?.UtcDateTime ?? DateTime.UtcNow;
        }
    }
}
