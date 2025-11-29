using System;
using Hangfire.Monitor.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hangfire.Monitor
{
    public static class HangfireMonitorExtensions
    {
        public static IServiceCollection AddHangfireMonitor(this IServiceCollection services, string name, string apiKey)
        {
            return services
                .AddHangfireMonitor(name, apiKey, null);
        }
        
        public static IServiceCollection AddHangfireMonitor(this IServiceCollection services, string name, string apiKey, Uri apiBaseUrl)
        {
            return services
                .AddSingleton<IStatisticsPublisher>(provider => new StatisticsPublisher(name, apiKey, apiBaseUrl, provider.GetService<ILogger<StatisticsPublisher>>()))
                .AddSingleton(provider => new StatisticsService(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30), provider.GetRequiredService<IStatisticsPublisher>()))
                .AddHostedService<HangfireMonitorHostedService>();
        }
    }
}