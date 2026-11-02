using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Soenneker.Flywheel.Communication.Dtos;
using Soenneker.Flywheel.Core.Services.Abstract;
using Soenneker.Messages.MsTeams;
using Soenneker.MsTeams.Sender.Abstract;
using Soenneker.Utils.Json;

namespace Soenneker.ServiceBus.Receptors.MsTeams;

internal sealed class MsTeamsJobInvoker : IJobInvoker
{
    internal static readonly JobDefinition<MsTeamsMessage> Definition = new("soenneker.msteams.send.v1");

    public string Name => Definition.Name;

    public async Task Invoke(IServiceProvider services, string payload, CancellationToken cancellationToken)
    {
        MsTeamsMessage message = JsonUtil.Deserialize(payload, MsTeamsJsonContext.Default.MsTeamsMessage)
            ?? throw new SerializationException("A Microsoft Teams job payload is required.");
        await services.GetRequiredService<IMsTeamsSender>().SendMessage(message, cancellationToken);
    }
}
