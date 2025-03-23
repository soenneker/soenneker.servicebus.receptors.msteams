using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.ServiceBus.Queue.Registrars;
using Soenneker.ServiceBus.Receptors.MsTeams.Abstract;

namespace Soenneker.ServiceBus.Receptors.MsTeams.Registrars;

/// <summary>
/// A Hangfire-integrated Service Bus message receptor that deserializes incoming Microsoft Teams messages and enqueues them for webhook processing using a background job.
/// </summary>
public static class MsTeamsReceptorRegistrar
{
    /// <summary>
    /// Adds <see cref="IMsTeamsReceptor"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddMsTeamsReceptorAsSingleton(this IServiceCollection services)
    {
        services.AddServiceBusQueueUtilAsSingleton();
        services.TryAddSingleton<IMsTeamsReceptor, MsTeamsReceptor>();

        return services;
    }
}
