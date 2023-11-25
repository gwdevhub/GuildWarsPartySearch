using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GuildWarsPartySearch.Server.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MTSC;
using MTSC.ServerSide;
using MTSC.ServerSide.Handlers;
using System.Extensions;
using System.Logging;

namespace GuildWarsPartySearch.Server.ServerHandlers;

public sealed class ContentManagementHandler : IHandler
{
    private bool initialized = false;
    private DateTime lastUpdateTime = DateTime.MinValue;
    private TimeSpan updateFrequency = TimeSpan.FromMinutes(5);
    private string stagingFolder = "Content";

    public void ClientRemoved(MTSC.ServerSide.Server server, ClientData client)
    {
    }

    public bool HandleClient(MTSC.ServerSide.Server server, ClientData client) => false;

    public bool HandleReceivedMessage(MTSC.ServerSide.Server server, ClientData client, Message message) => false;

    public bool HandleSendMessage(MTSC.ServerSide.Server server, ClientData client, ref Message message) => false;

    public bool PreHandleReceivedMessage(MTSC.ServerSide.Server server, ClientData client, ref Message message) => false;

    public void Tick(MTSC.ServerSide.Server server)
    {
        if (!this.initialized)
        {
            this.initialized = true;
            var options = server.ServiceManager.GetRequiredService<IOptions<ContentOptions>>();
            this.updateFrequency = options.Value.UpdateFrequency;
            this.stagingFolder = options.Value.StagingFolder;
        }

        if (DateTime.Now - this.lastUpdateTime > this.updateFrequency)
        {
            this.lastUpdateTime = DateTime.Now;
            var logger = server.ServiceManager.GetRequiredService<ILogger<ContentManagementHandler>>();
            var options = server.ServiceManager.GetRequiredService<IOptions<StorageAccountOptions>>();
            Task.Run(() => this.UpdateContentSafe(options.Value, logger));
            
        }
    }

    private async void UpdateContentSafe(StorageAccountOptions options, ILogger<ContentManagementHandler> logger)
    {
        var scopedLogger = logger.CreateScopedLogger(nameof(this.UpdateContentSafe), string.Empty);
        try
        {
            await this.UpdateContent(options, scopedLogger);
        }
        catch (Exception ex)
        {
            scopedLogger.LogError(ex, "Encountered exception");
        }
    }

    private async Task UpdateContent(StorageAccountOptions options, ScopedLogger<ContentManagementHandler> scopedLogger)
    {
        scopedLogger.LogInformation("Checking content to retrieve");
        var serviceBlobClient = new BlobServiceClient(options.ConnectionString);
        var blobContainerClient = serviceBlobClient.GetBlobContainerClient(options.ContainerName);
        var blobs = blobContainerClient.GetBlobsAsync();

        if (!Directory.Exists(this.stagingFolder))
        {
            Directory.CreateDirectory(this.stagingFolder);
        }

        scopedLogger.LogInformation($"Retrieving blobs");
        var blobList = new List<BlobItem>();
        await foreach(var blob in blobs)
        {
            blobList.Add(blob);
        }

        var stagingFolderFullPath = Path.GetFullPath(this.stagingFolder);
        var stagedFiles = Directory.GetFiles(this.stagingFolder, "*", SearchOption.AllDirectories);
        var filesToDelete = stagedFiles
            .Select(f => Path.GetFullPath(f).Replace(stagingFolderFullPath, "").Replace('\\', '/').Trim('/'))
            .Where(f => blobList.None(b => b.Name == f));
        foreach (var file in filesToDelete)
        {
            scopedLogger.LogInformation($"[{file}] File not in blob. Deleting");
            File.Delete($"{stagingFolderFullPath}\\{file}");
        }

        foreach(var blob in blobList)
        {
            var finalPath = Path.Combine(this.stagingFolder, blob.Name);
            var fileInfo = new FileInfo(finalPath);
            fileInfo.Directory!.Create();
            if (fileInfo.Exists &&
                fileInfo.CreationTimeUtc == blob.Properties.LastModified?.UtcDateTime &&
                fileInfo.Length == blob.Properties.ContentLength)
            {
                scopedLogger.LogInformation($"[{blob.Name}] File unchanged. Skipping");
                continue;
            }

            var blobClient = blobContainerClient.GetBlobClient(blob.Name);
            using var fileStream = new FileStream(finalPath, FileMode.Create);
            using var blobStream = await blobClient.OpenReadAsync(new BlobOpenReadOptions(false)
            {
                BufferSize = 1024,
            }, CancellationToken.None);

            scopedLogger.LogInformation($"[{blob.Name}] Downloading blob");
            await blobStream.CopyToAsync(fileStream);
            scopedLogger.LogInformation($"[{blob.Name}] Downloaded blob");

            fileInfo = new FileInfo(finalPath);
            fileInfo.CreationTimeUtc = blob.Properties.LastModified?.UtcDateTime ?? DateTime.UtcNow;
        }
    }
}
