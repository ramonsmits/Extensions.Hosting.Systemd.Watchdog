using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Systemd;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Extension methods for setting up <see cref="SystemdLifetime" />.
    /// </summary>
    public static class WatchdogHostBuilderExtensions
    {
        /// <summary>
        /// Sets the host lifetime to <see cref="SystemdLifetime" />,
        /// provides notification messages for application started and stopping,
        /// and configures console logging to the systemd format.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     This is context aware and will only activate if it detects the process is running
        ///     as a systemd Service.
        ///   </para>
        ///   <para>
        ///     The systemd service file must be configured with <c>Type=notify</c> to enable
        ///     notifications. See https://www.freedesktop.org/software/systemd/man/systemd.service.html.
        ///   </para>
        /// </remarks>
        /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to use.</param>
        /// <param name="enableWatchdog"></param>
        /// <returns></returns>
        public static IHostBuilder UseSystemd(this IHostBuilder hostBuilder, bool enableWatchdog = true)
        {
            hostBuilder.UseSystemd();
            hostBuilder.ConfigureServices(services => services.AddSystemd(enableWatchdog));
            return hostBuilder;
        }

#if NET8_0_OR_GREATER
        public static IHostApplicationBuilder UseSystemd(this IHostApplicationBuilder hostBuilder, bool enableWatchdog = true)
        {
            hostBuilder.Services.AddSystemd(enableWatchdog);
            return hostBuilder;
        }
#endif

        public static IServiceCollection AddSystemd(this IServiceCollection services, bool enableWatchdog)
        {
            var isSystemdService = SystemdHelpers.IsSystemdService();
            var isSystemdChildService = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NOTIFY_SOCKET")) || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LISTEN_PID"));

            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var logger = scope.ServiceProvider.GetService<ILogger<WatchdogService>>();
                logger?.LogInformation("UseSystemd {IsSystemdService} {EnableWatchdog} {IsSystemdChildService}", isSystemdService, enableWatchdog, isSystemdChildService);
            }

            if (enableWatchdog)
            {

                if (!isSystemdService && isSystemdChildService)
                {
                    // Workaround when process isn't main process but service child process
                    // See https://source.dot.net/#Microsoft.Extensions.Hosting.Systemd/SystemdNotifier.cs
                    services.AddSingleton<ISystemdNotifier, SystemdNotifier>();
                    // See https://source.dot.net/#Microsoft.Extensions.Hosting.Systemd/SystemdLifetime.cs
                    services.AddSingleton<IHostLifetime, SystemdLifetime>();
                }

                if (isSystemdService || isSystemdChildService)
                {
                    services.AddHostedService<WatchdogService>();
                }
            }
            return services;
        }
    }
}