using System.Logging;

namespace GuildWarsPartySearch.Server.Services.Logging
{
    public sealed class ConsoleLogger : ILogsWriter
    {
        public ConsoleLogger()
        {
        }

        public void WriteLog(Log log)
        {
            Console.WriteLine($"[{log.LogLevel}] [{log.LogTime.ToString("s")}] [{log.Category}] [{log.CorrelationVector}]\n{log.Message}\n{log.Exception}");
        }
    }
}
