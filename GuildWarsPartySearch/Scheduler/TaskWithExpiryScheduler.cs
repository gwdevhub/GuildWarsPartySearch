using MTSC;
using MTSC.Common;
using MTSC.ServerSide;
using MTSC.ServerSide.BackgroundServices;
using MTSC.ServerSide.Schedulers;

namespace GuildWarsPartySearch.Server.Scheduler;

public sealed class TaskWithExpiryScheduler : IScheduler
{
    private static readonly TimeSpan OperationTimeout = TimeSpan.FromSeconds(5);
    private static readonly TaskFactory TaskFactory = new();

    public void ScheduleBackgroundService(BackgroundServiceBase backgroundServiceBase)
    {
        try
        {
            TaskFactory.StartNew(backgroundServiceBase.Execute, new CancellationTokenSource(OperationTimeout).Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }
        catch
        {
        }
    }

    public void ScheduleHandling(List<(ClientData, IConsumerQueue<Message>)> clientsQueues, Action<ClientData, IConsumerQueue<Message>> messageHandlingProcedure)
    {
        var cancellationTokenSource = new CancellationTokenSource(OperationTimeout);
        foreach (var clientsQueue in clientsQueues)
        {
            var (client, messageQueue) = clientsQueue;
            try
            {
                TaskFactory.StartNew(() => messageHandlingProcedure(client, messageQueue), cancellationTokenSource.Token, TaskCreationOptions.PreferFairness, TaskScheduler.Current);
            }
            catch
            {
            }
        }
    }
}
