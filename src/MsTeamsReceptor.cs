using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Soenneker.Enums.JsonLibrary;
using Soenneker.Messages.MsTeams;
using Soenneker.MsTeams.Sender.Abstract;
using Soenneker.ServiceBus.Client.Abstract;
using Soenneker.ServiceBus.Queue.Abstract;
using Soenneker.ServiceBus.Receptor;
using Soenneker.ServiceBus.Receptors.MsTeams.Abstract;
using Soenneker.Utils.Json;

namespace Soenneker.ServiceBus.Receptors.MsTeams;

///<inheritdoc cref="IMsTeamsReceptor"/>
public sealed class MsTeamsReceptor : ServiceBusReceptor, IMsTeamsReceptor
{
    public MsTeamsReceptor(IServiceBusClientUtil serviceBusClientUtil, IServiceBusQueueUtil serviceBusQueueUtil, ILogger<MsTeamsReceptor> logger,
        IConfiguration config) : base("msteams", logger, serviceBusClientUtil, serviceBusQueueUtil, config)
    {
    }

    public override ValueTask OnMessageReceived(string messageContent, Type? type, CancellationToken cancellationToken = default)
    {
        try
        {
            // Cannot be System.Text.Json just yet because of the JsonConverter on the AdaptiveCard library class
            var msgModel = JsonUtil.Deserialize<MsTeamsMessage>(messageContent, JsonLibraryType.Newtonsoft);

            if (msgModel == null)
                throw new SerializationException($"Could not deserialize {nameof(MsTeamsMessage)} message content");

            _ = BackgroundJob.Enqueue<IMsTeamsSender>(x => x.SendMessage(msgModel, CancellationToken.None));
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Unable to queue job with content: {content}", messageContent);
        }

        return ValueTask.CompletedTask;
    }
}