using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var host = WatchdogHostBuilderExtensions.UseSystemd(Host.CreateDefaultBuilder(args)).ConfigureServices(((context, services) =>
{
    var configuration = context.Configuration;
    services.Configure<Options>(configuration.GetSection("Options"));
    services.AddHostedService<Worker>();
    services.AddSingleton<IHealthCheck, ActivityMonitor>();
})).Build();

host.Services.GetRequiredService<IOptions<Options>>();
await host.RunAsync();