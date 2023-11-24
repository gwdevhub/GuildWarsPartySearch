using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace GuildWarsPartySearch.Server.Services.Options;

public sealed class JsonOptionsManager : IOptionsManager
{
    private const string ConfigurationFile = "Config.json";
    private static readonly Dictionary<string, JToken?> Configuration = [];

    public JsonOptionsManager()
    {
        LoadConfiguration();
    }

    public T GetOptions<T>() where T : class
    {
        if (!Configuration.TryGetValue(typeof(T).Name, out var token) ||
            token is null)
        {
            throw new InvalidOperationException($"Unable to find entry {typeof(T).Name}");
        }

        return token.ToObject<T>() ?? throw new InvalidOperationException($"Unable to deserialize entry {typeof(T).Name}");
    }

    public void UpdateOptions<T>(T value) where T : class
    {
        throw new NotImplementedException();
    }

    private static void LoadConfiguration()
    {
        Configuration.Clear();
        if (!File.Exists(ConfigurationFile))
        {
            throw new InvalidOperationException("Unable to load configuration");
        }

        var config = File.ReadAllText(ConfigurationFile);
        var configObj = JsonConvert.DeserializeObject<JObject>(config);
        if (configObj is null)
        {
            throw new InvalidOperationException("Unable to load configuration");
        }

        foreach(var prop in configObj)
        {
            Configuration[prop.Key] = prop.Value;
        }
    }
}
