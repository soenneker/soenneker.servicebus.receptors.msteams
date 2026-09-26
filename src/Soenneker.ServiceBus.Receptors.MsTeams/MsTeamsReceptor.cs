using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Soenneker.Messages.MsTeams;
using Soenneker.MsTeams.Sender.Abstract;
using Soenneker.ServiceBus.Client.Abstract;
using Soenneker.ServiceBus.Queue.Abstract;
using Soenneker.ServiceBus.Receptor;
using Soenneker.ServiceBus.Receptors.MsTeams.Abstract;
using Soenneker.Utils.Json;

namespace Soenneker.ServiceBus.Receptors.MsTeams;

public sealed class MsTeamsReceptor : ServiceBusReceptor, IMsTeamsReceptor
{
    private static readonly MethodInfo _sendMethod = typeof(IMsTeamsSender).GetMethod(nameof(IMsTeamsSender.SendMessage), [typeof(MsTeamsMessage), typeof(CancellationToken)])!;
    private static readonly object _jobCancellationToken = CancellationToken.None;
    private readonly IBackgroundJobClient? _backgroundJobClient;

    public MsTeamsReceptor(IServiceBusClientUtil serviceBusClientUtil, IServiceBusQueueUtil serviceBusQueueUtil, ILogger<MsTeamsReceptor> logger,
        IConfiguration config, IBackgroundJobClient backgroundJobClient) : this(serviceBusClientUtil, serviceBusQueueUtil, logger, config)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public MsTeamsReceptor(IServiceBusClientUtil serviceBusClientUtil, IServiceBusQueueUtil serviceBusQueueUtil, ILogger<MsTeamsReceptor> logger,
        IConfiguration config) : base("msteams", logger, serviceBusClientUtil, serviceBusQueueUtil, config)
    {
    }

    public override ValueTask OnMessageReceived(string messageContent, string type, CancellationToken cancellationToken = default)
    {
        try
        {
            var msgModel = JsonUtil.Deserialize(messageContent, MsTeamsJsonContext.Default.MsTeamsMessage);

            if (msgModel == null)
                throw new SerializationException($"Could not deserialize {nameof(MsTeamsMessage)} message content");

            _ = (_backgroundJobClient ?? new BackgroundJobClient()).Create(new Job(typeof(IMsTeamsSender), _sendMethod, [msgModel, _jobCancellationToken]), new EnqueuedState());
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Unable to enqueue Microsoft Teams job for message type {type}", type);
            throw;
        }

        return ValueTask.CompletedTask;
    }
}
