using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Flywheel.Core.Services.Abstract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Soenneker.Messages.MsTeams;
using Soenneker.ServiceBus.Client.Abstract;
using Soenneker.ServiceBus.Queue.Abstract;
using Soenneker.ServiceBus.Receptor;
using Soenneker.ServiceBus.Receptors.MsTeams.Abstract;
using Soenneker.Utils.Json;

namespace Soenneker.ServiceBus.Receptors.MsTeams;

public sealed class MsTeamsReceptor : ServiceBusReceptor, IMsTeamsReceptor
{
    private readonly IJobClient _jobs;

    public MsTeamsReceptor(IServiceBusClientUtil serviceBusClientUtil, IServiceBusQueueUtil serviceBusQueueUtil, ILogger<MsTeamsReceptor> logger,
        IConfiguration config, IJobClient jobs) : base("msteams", logger, serviceBusClientUtil, serviceBusQueueUtil, config)
    {
        _jobs = jobs;
    }

    public override async ValueTask OnMessageReceived(string messageContent, string type, CancellationToken cancellationToken = default)
    {
        try
        {
            var msgModel = JsonUtil.Deserialize(messageContent, MsTeamsJsonContext.Default.MsTeamsMessage);

            if (msgModel == null)
                throw new SerializationException($"Could not deserialize {nameof(MsTeamsMessage)} message content");

            await _jobs.Enqueue(MsTeamsJobInvoker.Definition, msgModel, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Unable to enqueue Microsoft Teams job for message type {type}", type);
            throw;
        }
    }
}
