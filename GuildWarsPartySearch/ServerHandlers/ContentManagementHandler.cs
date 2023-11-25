using Azure.Storage.Blobs;
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
        scopedLogger.LogInformation("Retrieving content");
        var serviceBlobClient = new BlobServiceClient(options.ConnectionString);
        var blobContainerClient = serviceBlobClient.GetBlobContainerClient(options.ContainerName);
        var blobs = blobContainerClient.GetBlobsAsync();

        //TODO: Implement a better update. Currently we simply delete everything and redownload all the content
        scopedLogger.LogInformation($"[{this.stagingFolder}] Deleting staging folder if it exists");
        if (Directory.Exists(this.stagingFolder))
        {
            Directory.Delete(this.stagingFolder, true);
        }

        scopedLogger.LogInformation($"Retrieving blobs");
        await foreach(var blob in blobs)
        {
            var finalPath = Path.Combine(this.stagingFolder, blob.Name);
            var fileInfo = new FileInfo(finalPath);
            fileInfo.Directory!.Create();
            var blobClient = blobContainerClient.GetBlobClient(blob.Name);
            using var fileStream = new FileStream(finalPath, FileMode.Create);
            using var blobStream = await blobClient.OpenReadAsync(new Azure.Storage.Blobs.Models.BlobOpenReadOptions(false)
            {
                BufferSize = 1024,
            }, CancellationToken.None);

            scopedLogger.LogInformation($"[{blob.Name}] Downloading blob");
            await blobStream.CopyToAsync(fileStream);
            scopedLogger.LogInformation($"[{blob.Name}] Downloaded blob");
        }
    }
}
