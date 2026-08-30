using Soenneker.ServiceBus.Receptor.Abstract;

namespace Soenneker.ServiceBus.Receptors.MsTeams.Abstract;

/// <summary>
/// Consumes <c>msteams</c> queue messages, deserializes <c>MsTeamsMessage</c> bodies, and enqueues them for <c>IMsTeamsSender</c> processing through Hangfire.
/// </summary>
public interface IMsTeamsReceptor : IServiceBusReceptor
{
}
