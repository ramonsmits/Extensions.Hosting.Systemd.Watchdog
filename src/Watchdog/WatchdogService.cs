﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Systemd;
using Microsoft.Extensions.Logging;

class WatchdogService : IHostedService, IDisposable
{
    static readonly ServiceState WatchDogState = new("WATCHDOG=1");
    readonly ISystemdNotifier SystemdNotifier;
    readonly IWatchdogCheck[] Checks;
    readonly Timer watchdogTimer;
    readonly TimeSpan Interval;
    readonly ILogger Logger;

    const long WatchdogFrequency = 1000000L;

    public WatchdogService(ILogger<WatchdogService> logger, ISystemdNotifier systemdNotifier, IEnumerable<IWatchdogCheck> healthChecks)
    {
        Logger = logger;
        Checks = healthChecks.ToArray();
        SystemdNotifier = systemdNotifier;

        if (Checks.Length == 0) throw new InvalidOperationException($"Atleast one `{nameof(IWatchdogCheck)}` needs to be registered.");

        var WATCHDOG_USEC = Environment.GetEnvironmentVariable("WATCHDOG_USEC");

        Logger.LogDebug("WATCHDOG_USEC={WATCHDOG_USEC}", WATCHDOG_USEC);

        if (WATCHDOG_USEC == null)
        {
            return;
        }

        // Watchdog timeout is 1 second, so need to update within that duration. Good value is to half the timeout (div.
        Interval = TimeSpan.FromSeconds(long.Parse(WATCHDOG_USEC) / 2D / WatchdogFrequency);

        Logger.LogInformation("Interval={Interval}", Interval);

        watchdogTimer = new Timer(x =>
        {
            var allHealthy = Checks.All(y => y.IsHealthy);
            if (allHealthy) SystemdNotifier.Notify(WatchDogState);
        });
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        watchdogTimer?.Change(Interval, Interval);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        watchdogTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        watchdogTimer?.Dispose();
    }
}