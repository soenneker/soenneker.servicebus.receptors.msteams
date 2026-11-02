using Microsoft.Extensions.DependencyInjection;
using Soenneker.Flywheel.Core.Services.Abstract;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.MsTeams.Sender.Registrars;
using Soenneker.ServiceBus.Queue.Registrars;
using Soenneker.ServiceBus.Receptors.MsTeams.Abstract;

namespace Soenneker.ServiceBus.Receptors.MsTeams.Registrars;

/// <summary>
/// Registers the Flywheel-backed <c>msteams</c> queue receptor, Teams sender, and Service Bus dependencies.
/// </summary>
public static class MsTeamsReceptorRegistrar
{
    /// <summary>
    /// Adds <see cref="IMsTeamsReceptor"/> as a singleton service. <para/>
    /// </summary>
    /// <remarks>The host must configure Flywheel with a storage provider and include MsTeamsMessage in its payload JSON context.</remarks>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddMsTeamsReceptorAsSingleton(this IServiceCollection services)
    {
        services.AddMsTeamsSenderAsScoped().AddServiceBusQueueUtilAsSingleton().TryAddSingleton<IMsTeamsReceptor, MsTeamsReceptor>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IJobInvoker, MsTeamsJobInvoker>());

        return services;
    }
}
