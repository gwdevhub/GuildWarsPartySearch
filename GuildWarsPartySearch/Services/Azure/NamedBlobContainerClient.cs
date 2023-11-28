using Azure;
using Azure.Core;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GuildWarsPartySearch.Server.Options.Azure;

namespace GuildWarsPartySearch.Server.Services.Azure;

public class NamedBlobContainerClient<TOptions> : BlobContainerClient
    where TOptions : IAzureBlobStorageOptions
{
    public NamedBlobContainerClient(string connectionString, string blobContainerName) : base(connectionString, blobContainerName)
    {
    }

    public NamedBlobContainerClient(Uri blobContainerUri, BlobClientOptions? options = null) : base(blobContainerUri, options)
    {
    }

    public NamedBlobContainerClient(string connectionString, string blobContainerName, BlobClientOptions options) : base(connectionString, blobContainerName, options)
    {
    }

    public NamedBlobContainerClient(Uri blobContainerUri, StorageSharedKeyCredential credential, BlobClientOptions? options = null) : base(blobContainerUri, credential, options)
    {
    }

    public NamedBlobContainerClient(Uri blobContainerUri, AzureSasCredential credential, BlobClientOptions? options = null) : base(blobContainerUri, credential, options)
    {
    }

    public NamedBlobContainerClient(Uri blobContainerUri, TokenCredential credential, BlobClientOptions? options = null) : base(blobContainerUri, credential, options)
    {
    }

    protected NamedBlobContainerClient()
    {
    }
}
