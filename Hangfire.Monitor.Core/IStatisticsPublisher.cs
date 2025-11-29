using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hangfire.Monitor.Core
{
    public interface IStatisticsPublisher : IDisposable
    {
        Task PublishAsync(CancellationToken cancellationToken = default);
    }
}