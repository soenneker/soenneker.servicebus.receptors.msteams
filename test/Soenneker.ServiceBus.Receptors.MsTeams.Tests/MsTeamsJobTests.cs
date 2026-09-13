using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Configuration;

namespace Audit;

public class MsTeamsJobTests
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
    [Test]
    public async Task TeamsJobPreservesDeserializedPayloadAndFailurePropagation()
    {
        var client = new RecordingJobs();
        var receptor = new Soenneker.ServiceBus.Receptors.MsTeams.MsTeamsReceptor(null!, null!,
            NullLogger<Soenneker.ServiceBus.Receptors.MsTeams.MsTeamsReceptor>.Instance, Fixture.Config(), client);
        await receptor.OnMessageReceived("{\"msTeamsCard\":{\"type\":\"message\",\"attachments\":[]},\"channel\":\"audit\"}", "teams");
        Check(client.Job!.Type == typeof(Soenneker.MsTeams.Sender.Abstract.IMsTeamsSender) && client.Job.Method.Name == "SendMessage", "Wrong Teams target");
        Check(((Soenneker.Messages.MsTeams.MsTeamsMessage)client.Job.Args[0]).Channel == "audit", "Teams payload lost");
        Check((CancellationToken)client.Job.Args[1] == CancellationToken.None, "Wrong Teams cancellation token");
        try { await receptor.OnMessageReceived("invalid", "teams"); throw new Exception("Invalid JSON was swallowed"); }
        catch (Newtonsoft.Json.JsonException) { }
    }
    private sealed class RecordingJobs : IBackgroundJobClient
    {
        public Job? Job; public IState? State;
        public string Create(Job job, IState state) { Job = job; State = state; return "1"; }
        public bool ChangeState(string jobId, IState state, string expectedState) => true;
    }
}

internal static class Fixture
{
    public static Microsoft.Extensions.Configuration.IConfiguration Config(bool logging = false, bool counts = true) =>
        new Microsoft.Extensions.Configuration.ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Azure:ServiceBus:Enable"] = "true", ["Azure:ServiceBus:TransmitterLogging"] = logging.ToString(),
            ["Background:QueueLength"] = "32", ["Background:LockCounts"] = counts.ToString()
        }).Build();
}
