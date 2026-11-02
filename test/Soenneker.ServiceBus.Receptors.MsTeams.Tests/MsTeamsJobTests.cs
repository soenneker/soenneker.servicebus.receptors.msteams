using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Soenneker.Flywheel.Communication.Dtos;
using Soenneker.Flywheel.Core.Services.Abstract;
using Soenneker.Messages.MsTeams;
using Soenneker.MsTeams.Sender.Abstract;
using Soenneker.ServiceBus.Receptors.MsTeams.Registrars;

namespace Soenneker.ServiceBus.Receptors.MsTeams.Tests;

public class MsTeamsJobTests
{
    private const string Payload = """
        {"type":"msteams","id":"audit-id","queue":"msteams","sender":"audit","createdAt":"2026-01-01T00:00:00Z","msTeamsCard":{"type":"message","attachments":[]},"channel":"audit"}
        """;

    private static MsTeamsReceptor CreateReceptor(IJobClient jobs)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Azure:ServiceBus:Enable"] = "true", ["Background:QueueLength"] = "32"
        }).Build();
        return new MsTeamsReceptor(null!, null!, NullLogger<MsTeamsReceptor>.Instance, config, jobs);
    }

    [Test]
    public async Task TeamsJobPreservesDeserializedPayloadAndCancellation(CancellationToken cancellationToken)
    {
        var jobs = Substitute.For<IJobClient>();
        await CreateReceptor(jobs).OnMessageReceived(Payload, "teams", cancellationToken);
        await jobs.Received(1).Enqueue(Arg.Is<JobDefinition<MsTeamsMessage>>(j => j.Name == "soenneker.msteams.send.v1"),
            Arg.Is<MsTeamsMessage>(m => m.Id == "audit-id" && m.Channel == "audit" && m.MsTeamsCard != null),
            cancellationToken: cancellationToken);
    }

    [Test]
    [Arguments("invalid")]
    [Arguments("null")]
    public async Task InvalidPayloadDoesNotEnqueue(string payload)
    {
        var jobs = Substitute.For<IJobClient>();
        bool failed = false;
        try { await CreateReceptor(jobs).OnMessageReceived(payload, "teams"); }
        catch (Exception e) when (e is JsonException or SerializationException) { failed = true; }
        if (!failed || jobs.ReceivedCalls().Any()) throw new InvalidOperationException("Invalid payload was accepted.");
    }

    [Test]
    public async Task EnqueueFailurePropagatesAfterPersistenceCompletes()
    {
        var jobs = Substitute.For<IJobClient>();
        var pending = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        jobs.Enqueue(Arg.Any<JobDefinition<MsTeamsMessage>>(), Arg.Any<MsTeamsMessage>()).Returns(pending.Task);
        Task handling = CreateReceptor(jobs).OnMessageReceived(Payload, "teams").AsTask();
        if (handling.IsCompleted) throw new InvalidOperationException("Message completed before persistence.");
        var failure = new InvalidOperationException("Store unavailable");
        pending.SetException(failure);
        try { await handling; }
        catch (InvalidOperationException e) when (ReferenceEquals(e, failure)) { return; }
        throw new InvalidOperationException("Enqueue failure was swallowed.");
    }

    [Test]
    public async Task RegisteredJobInvokesScopedSender(CancellationToken cancellationToken)
    {
        var sender = Substitute.For<IMsTeamsSender>();
        var services = new ServiceCollection();
        services.AddScoped<IMsTeamsSender>(_ => sender);
        services.AddMsTeamsReceptorAsSingleton();
        services.AddMsTeamsReceptorAsSingleton();
        await using ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using IServiceScope scope = provider.CreateScope();
        IJobInvoker invoker = provider.GetServices<IJobInvoker>().Single();
        var message = JsonSerializer.Deserialize(Payload, MsTeamsJsonContext.Default.MsTeamsMessage)!;
        string persisted = JsonSerializer.Serialize(message, MsTeamsJsonContext.Default.MsTeamsMessage);
        await invoker.Invoke(scope.ServiceProvider, persisted, cancellationToken);
        await sender.Received(1).SendMessage(Arg.Is<MsTeamsMessage>(m => m.Channel == "audit" && m.Id == "audit-id"), cancellationToken);
    }
}
