using Soenneker.ServiceBus.Receptor.Abstract;

namespace Soenneker.ServiceBus.Receptors.MsTeams.Abstract;

/// <summary>
/// A Hangfire-integrated Service Bus message receptor that deserializes incoming Microsoft Teams messages and enqueues them for webhook processing using a background job.
/// </summary>
public interface IMsTeamsReceptor : IServiceBusReceptor
{
}
