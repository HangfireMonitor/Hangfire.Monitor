using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hangfire.Monitor.Core
{
    public class StatisticsService : IDisposable
    {
        private readonly TimeSpan _startupDelay;
        private readonly TimeSpan _interval;
        private readonly IStatisticsPublisher _statisticsPublisher;

        public StatisticsService(TimeSpan startupDelay, TimeSpan interval, IStatisticsPublisher statisticsPublisher)
        {
            _startupDelay = startupDelay;
            _interval = interval;
            _statisticsPublisher = statisticsPublisher ?? throw new ArgumentNullException(nameof(statisticsPublisher));
        }
        
        public async Task ProcessAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(_startupDelay, cancellationToken);
            
            while (!cancellationToken.IsCancellationRequested)
            {
                await _statisticsPublisher.PublishAsync(cancellationToken);
                await Task.Delay(_interval, cancellationToken);   
            }
        }
        
        public void Dispose()
        {
            _statisticsPublisher.Dispose();
        }
    }
}