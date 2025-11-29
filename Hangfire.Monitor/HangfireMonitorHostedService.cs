using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire.Monitor.Core;
using Microsoft.Extensions.Hosting;

namespace Hangfire.Monitor
{
    public class HangfireMonitorHostedService : BackgroundService
    {
        private readonly StatisticsService _statisticsService;

        public HangfireMonitorHostedService(StatisticsService statisticsService)
        {
            _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _statisticsService.ProcessAsync(stoppingToken);
        }
    }
}