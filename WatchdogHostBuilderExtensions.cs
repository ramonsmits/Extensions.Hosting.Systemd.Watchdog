using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Systemd;

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
            if (SystemdHelpers.IsSystemdService())
            {
                hostBuilder.ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<WatchdogService>();
                });
            }
            return hostBuilder;
        }
    }
}