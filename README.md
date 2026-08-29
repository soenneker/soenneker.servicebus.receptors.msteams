[![](https://img.shields.io/nuget/v/soenneker.servicebus.receptors.msteams.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.servicebus.receptors.msteams/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.servicebus.receptors.msteams/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.servicebus.receptors.msteams/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.servicebus.receptors.msteams.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.servicebus.receptors.msteams/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.servicebus.receptors.msteams/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.servicebus.receptors.msteams/actions/workflows/codeql.yml)

# Soenneker.ServiceBus.Receptors.MsTeams

A Hangfire-integrated Service Bus message receptor that deserializes incoming Microsoft Teams messages and enqueues them for webhook processing using a background job.

## Install

```bash
dotnet add package Soenneker.ServiceBus.Receptors.MsTeams
```

## Quick start

```csharp
using Soenneker.ServiceBus.Receptors.MsTeams.Registrars;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
var result = services.AddMsTeamsReceptorAsSingleton();
```

Adds `IMsTeamsReceptor` as a singleton service.

## What you get

- `IMsTeamsReceptor` — A Hangfire-integrated Service Bus message receptor that deserializes incoming Microsoft Teams messages and enqueues them for webhook processing using a background job.
- `MsTeamsReceptorRegistrar` — A Hangfire-integrated Service Bus message receptor that deserializes incoming Microsoft Teams messages and enqueues them for webhook processing using a background job.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `MsTeamsReceptorRegistrar.AddMsTeamsReceptorAsSingleton(services)` | Adds `IMsTeamsReceptor` as a singleton service. | The same service collection, so additional registrations can be chained. |
