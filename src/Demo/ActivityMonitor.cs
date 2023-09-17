﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading;

class ActivityMonitor : IHealthCheck
{
    readonly CancellationTokenSource Cts;
    readonly CancellationToken Ct;
    readonly ILogger Log;

    bool IHealthCheck.IsHealthy
    {
        get
        {
            bool isHealthy = !Ct.IsCancellationRequested;
            Log.LogInformation("IsHealthy = {result}", isHealthy);
            return isHealthy;
        }
    }

    public ActivityMonitor(ILogger<ActivityMonitor> logger, IOptions<Options> options)
    {
        Cts = new CancellationTokenSource(options.Value.Delay);
        Ct = Cts.Token;
        Log = logger;
    }
}