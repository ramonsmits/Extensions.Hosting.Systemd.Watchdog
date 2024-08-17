using System;

[Obsolete("Use IWatchdogCheck", true)]
public interface IHealthCheck
{
    bool IsHealthy { get; }
}