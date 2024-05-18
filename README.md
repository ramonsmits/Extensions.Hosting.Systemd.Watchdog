# Extensions.Hosting.Systemd.Watchdog

Adds support for Systemd Watchdog to [Microsoft.Extensions.Hosting.Systemd](https://www.nuget.org/packages/Microsoft.Extensions.Hosting.Systemd)

## Usage

### Installation

Add the packages [Extensions.Hosting.Systemd.Watchdog](https://www.nuget.org/packages/Extensions.Hosting.Systemd.Watchdog) which is hosted on NuGet.

### Enabling watchdog:

Invoke the `UseSystemd` overload that takes a bool.

```c#
var host = new HostBuilder()
    .UseSystemd(enableWatchdog: true)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton<IHealthCheck>(new SignalHealthCheck(TimeSpan.FromMinutes(5)));
    })
```

or when using the .net 8 `HostApplicationBuilder`:

```c#
var builder = Host.CreateApplicationBuilder(args);
builder.UseSystemd(enableWatchdog: true);
builder.Services.AddSingleton<IHealthCheck>(new SignalHealthCheck(TimeSpan.FromMinutes(5));
```

### Register health checks

The watchdog needs an implementation for `IHeathCheck` and the results of its `IsHealhty` property is used to notify the state to systemd. Register at least 1 `IHealthCheck` implementation. The watchdog routing will probe all registered implementations.

```c#
var host = new HostBuilder()
    .UseSystemd(enableWatchdog: true)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton<IHealthCheck>(new SignalHealthCheck(TimeSpan.FromMinutes(5)));
    })
```

Not registering at least one health check will result in a startup failure.

### Example health check

The following example implementation relies on the application that a critical component will frequently "signal" that they are still healthy. For example, signaling after a certain task to complete successfully. The approach is very useful if the task can actually take a long time to complete to not interfere with the watchdog interval.

```c#
sealed class SignalHealthCheck : IHealthCheck
{
    readonly TimeSpan InactivityThreshold;

    DateTime healthExpiration;

    bool IHealthCheck.IsHealthy => DateTime.UtcNow < healthExpiration;

    public SignalHealthCheck(TimeSpan inactivityThreshold)
    {
        InactivityThreshold = inactivityThreshold;
        healthExpiration = DateTime.UtcNow + NoActivityAlertDuration;
    }

    public void Signal()
    {
        healthExpiration = DateTime.UtcNow + NoActivityAlertDuration;
    }
}
```

## Background

A services was critical to a system but frequently stalls. I wanted to use the watchdog feature to ensure the service gets restarted by systemctl.
