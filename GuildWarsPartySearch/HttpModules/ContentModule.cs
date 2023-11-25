using GuildWarsPartySearch.Server.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MTSC.Common.Http;
using MTSC.Common.Http.ServerModules;
using MTSC.ServerSide;
using MTSC.ServerSide.Handlers;
using System.Extensions;

namespace GuildWarsPartySearch.Server.HttpModules;

public sealed class ContentModule : IHttpModule
{
    private ContentOptions? contentOptions;

    public bool HandleRequest(MTSC.ServerSide.Server server, HttpHandler handler, ClientData client, HttpRequest request, ref HttpResponse response)
    {
        if (this.contentOptions is null)
        {
            response = ContentUnavailable503;
            return true;
        }

        var path = request.RequestURI;
        var contentRootPath = Path.GetFullPath(this.contentOptions.StagingFolder);
        var requestUri = request.RequestURI;
        if (requestUri.IsNullOrWhiteSpace() ||
            requestUri == "/")
        {
            requestUri = "index.html";
        }

        var requestedPath = Path.Combine(contentRootPath, requestUri);
        if (!IsSubPathOf(contentRootPath, requestedPath))
        {
            response = Forbidden403;
            return true;
        }

        if (!File.Exists(requestedPath))
        {
            response = NotFound404;
            return true;
        }

        response = Ok200(File.ReadAllBytes(requestedPath));
        return true;
    }

    public void Tick(MTSC.ServerSide.Server server, HttpHandler handler)
    {
        if (this.contentOptions is null)
        {
            this.contentOptions = server.ServiceManager.GetRequiredService<IOptions<ContentOptions>>().Value;
        }
    }

    private static HttpResponse Ok200(byte[] content) => new()
    {
        StatusCode = HttpMessage.StatusCodes.OK,
        Body = content
    };

    private static readonly HttpResponse NotFound404 = new()
    {
        StatusCode = HttpMessage.StatusCodes.NotFound,
        BodyString = "Content not found"
    };

    private static readonly HttpResponse ContentUnavailable503 = new()
    {
        StatusCode = HttpMessage.StatusCodes.ServiceUnavailable,
        BodyString = "Content is not yet available. Please wait and try again"
    };

    private static readonly HttpResponse Forbidden403 = new()
    {
        StatusCode = HttpMessage.StatusCodes.Forbidden,
        BodyString = "Requested content is forbidden. Please adjust request uri and try again"
    };

    private static bool IsSubPathOf(string basePath, string path)
    {
        // Normalize the base path and the test path
        var normalizedBasePath = Path.GetFullPath(basePath);
        var normalizedTestPath = Path.GetFullPath(path);

        return normalizedTestPath.StartsWith(normalizedBasePath, StringComparison.OrdinalIgnoreCase);
    }
}
