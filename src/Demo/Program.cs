using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;


#if NET8_0_OR_GREATER

var builder = Host.CreateApplicationBuilder(args);
builder.UseSystemd();
builder.Services
    .AddHostedService<Worker>()
    .AddSingleton<IHealthCheck, ActivityMonitor>();

#else
var builder = Host.CreateDefaultBuilder(args)
    .UseSystemd()
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<Worker>();
        services.AddSingleton<IHealthCheck, ActivityMonitor>();
    });
#endif

using var host = builder.Build();

await host.RunAsync();
