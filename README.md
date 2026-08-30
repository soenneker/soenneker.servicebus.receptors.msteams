[![](https://img.shields.io/nuget/v/soenneker.servicebus.receptors.msteams.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.servicebus.receptors.msteams/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.servicebus.receptors.msteams/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.servicebus.receptors.msteams/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.servicebus.receptors.msteams.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.servicebus.receptors.msteams/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.servicebus.receptors.msteams/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.servicebus.receptors.msteams/actions/workflows/codeql.yml)

# Soenneker.ServiceBus.Receptors.MsTeams

Consumes messages from the `msteams` Azure Service Bus queue, deserializes `MsTeamsMessage` bodies, and hands them to `IMsTeamsSender` through Hangfire.

## Installation

```bash
dotnet add package Soenneker.ServiceBus.Receptors.MsTeams
```

## Prerequisites

Configure `Azure:ServiceBus:ConnectionString`. The credential needs queue-management and receive permissions because initialization creates the `msteams` queue when absent and starts a processor.

Configure Hangfire storage and run a Hangfire server. The registrar adds `IMsTeamsSender` as a scoped service so Hangfire can resolve it for each job.

## Register and start

```csharp
using Soenneker.ServiceBus.Receptors.MsTeams.Abstract;
using Soenneker.ServiceBus.Receptors.MsTeams.Registrars;

services.AddMsTeamsReceptorAsSingleton();
```

Registration alone does not start processing. Resolve and initialize the singleton receptor during application startup:

```csharp
IMsTeamsReceptor receptor =
    services.GetRequiredService<IMsTeamsReceptor>();

await receptor.Init(cancellationToken);
```

Dispose the receptor during shutdown so its Service Bus processor stops cleanly.

## Message contract

The Service Bus body must be a JSON representation of `Soenneker.Messages.MsTeams.MsTeamsMessage`. Newtonsoft.Json is used because the Adaptive Card model used by the message has a Newtonsoft converter.

After deserialization, the receptor enqueues this Hangfire call:

```csharp
IMsTeamsSender.SendMessage(
    message,
    CancellationToken.None)
```

The Service Bus `ApplicationProperties["type"]` value is accepted by the base receptor but is not used to choose a Teams DTO or sender path in this implementation.

The job intentionally receives `CancellationToken.None` rather than the short-lived Service Bus delivery token. Hangfire manages cancellation for the eventual background execution.

## Delivery behavior

The Service Bus message is completed only after its body is deserialized and Hangfire accepts the job. Deserialization or enqueue failures flow back to the processor, leaving the broker message unsettled for retry and eventual dead-lettering according to queue policy.

Delivery is at least once across the handoff. If Hangfire persists a job but Service Bus completion fails, another delivery can enqueue the same Teams message again. Use a stable message identifier or another idempotency mechanism when duplicate webhook posts are unacceptable.

Message bodies can contain webhook URLs and user-supplied card content. This receptor does not write failed bodies to its own error log; apply the same care to upstream Service Bus logging and Hangfire argument storage.
