using System.Net;
using Hangfire.Storage.Monitoring;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Contrib.HttpClient;
using Newtonsoft.Json;

namespace Hangfire.Monitor.Core.Tests;

public class StatisticsPublisherTests
{
    private const string Name = "TestApp";
    private HttpClient? _httpClient;
    private StatisticsPublisher? _statisticsPublisher;
    private Mock<HttpMessageHandler> _handler;
    private StatisticsDto? _statistics;
    private Mock<ILogger<StatisticsPublisher>>? _loggerMock;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<StatisticsPublisher>>();
        _handler = new Mock<HttpMessageHandler>();
        _httpClient = _handler.CreateClient();
        _httpClient.BaseAddress = new Uri("https://test");
        _statisticsPublisher = new StatisticsPublisher(Name, _httpClient, _loggerMock.Object);
        _statistics = new StatisticsDto
        {
            Deleted = 1,
            Enqueued = 2,
            Failed = 3,
            Processing = 4,
            Queues = 5,
            Recurring = 6,
            Scheduled = 7,
            Servers = 8,
            Succeeded = 9
        };
       
        _handler.SetupRequest(HttpMethod.Post, "https://test/api/statistics").ReturnsResponse(HttpStatusCode.Accepted);
    }
    
    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
        _statisticsPublisher?.Dispose();
    }

    [Test]
    public async Task PublishAsync_ShouldSendStatistics()
    {
        JobStorage.Current = new JobStorageStub(() => new MonitoringApiStub(() => _statistics));
        
        await _statisticsPublisher!.PublishAsync(CancellationToken.None);
        
        _handler.VerifyRequest(HttpMethod.Post, "https://test/api/statistics", async message =>
        {
            if (message.Content is not StringContent content) return false;
            var json = await content.ReadAsStringAsync();
            var payload = JsonConvert.DeserializeObject<Payload>(json);
            
            return payload != null &&
                   payload.Name == Name &&
                   payload.Deleted == _statistics!.Deleted &&
                   payload.Enqueued == _statistics.Enqueued &&
                   payload.Failed == _statistics.Failed &&
                   payload.Processing == _statistics.Processing &&
                   payload.Queues == _statistics.Queues &&
                   payload.Recurring == _statistics.Recurring &&
                   payload.Scheduled == _statistics.Scheduled &&
                   payload.Servers == _statistics.Servers &&
                   payload.Succeeded == _statistics.Succeeded;
        });
    }
    
    [Test]
    public async Task PublishAsync_ShouldLog_WhenJobStorageIsNotInitialized()
    {
        Hangfire.JobStorage.Current = null;
        
        await _statisticsPublisher!.PublishAsync(CancellationToken.None);
        
        _loggerMock!.VerifyLog(LogLevel.Warning, "Hangfire JobStorage is not initialized. Statistics will not be published.", Times.Once());
    }
    
    [Test]
    public async Task PublishAsync_ShouldNotSendStatistics__WhenJobStorageIsNotInitialized()
    {
        Hangfire.JobStorage.Current = null;
        
        await _statisticsPublisher!.PublishAsync(CancellationToken.None);
        
        _handler.VerifyRequest(HttpMethod.Post, "https://test/api/statistics", Times.Never(), "");
    }
    
    [Test]
    public async Task PublishAsync_ShouldLog_WhenMonitoringApiIsNotReturned()
    {
        Hangfire.JobStorage.Current = new JobStorageStub(() => null);
        
        await _statisticsPublisher!.PublishAsync(CancellationToken.None);
        
        _loggerMock!.VerifyLog(LogLevel.Warning, "Hangfire JobStorage did not return an IMonitoringApi instance. Statistics will not be published.", Times.Once());
    }
    
    [Test]
    public async Task PublishAsync_ShouldNotSendStatistics_WhenMonitoringApiIsNotReturned()
    {
        Hangfire.JobStorage.Current = new JobStorageStub(() => null);
        
        await _statisticsPublisher!.PublishAsync(CancellationToken.None);
        
        _handler.VerifyRequest(HttpMethod.Post, "https://test/api/statistics", Times.Never(), "");
    }
    
    [Test]
    public async Task PublishAsync_ShouldLog_WhenGettingMonitoringApiThrows()
    {
        Hangfire.JobStorage.Current = new JobStorageStub(() => throw new Exception());
        
        await _statisticsPublisher!.PublishAsync(CancellationToken.None);
        
        _loggerMock!.VerifyLog(LogLevel.Warning, "Hangfire JobStorage failed to return an IMonitoringApi instance. Statistics will not be published.", Times.Once());
    }
    
    [Test]
    public async Task PublishAsync_ShouldNotSendStatistics_WhenGettingMonitoringApiThrows()
    {
        Hangfire.JobStorage.Current = new JobStorageStub(() => throw new Exception());
        
        await _statisticsPublisher!.PublishAsync(CancellationToken.None);
        
        _handler.VerifyRequest(HttpMethod.Post, "https://test/api/statistics", Times.Never(), "");
    }
    
    [Test]
    public async Task PublishAsync_ShouldLog_WhenStatisticsAreNotReturned()
    {
        Hangfire.JobStorage.Current = new JobStorageStub(() => new MonitoringApiStub(() => null));
        
        await _statisticsPublisher!.PublishAsync(CancellationToken.None);
        
        _loggerMock!.VerifyLog(LogLevel.Warning, "Hangfire Monitoring API did not return statistics. Statistics will not be published.", Times.Once());
    }
    
    [Test]
    public async Task PublishAsync_ShouldNotSendStatistics_WhenStatisticsAreNotReturned()
    {
        Hangfire.JobStorage.Current = new JobStorageStub(() => new MonitoringApiStub(() => null));
        
        await _statisticsPublisher!.PublishAsync(CancellationToken.None);
        
        _handler.VerifyRequest(HttpMethod.Post, "https://test/api/statistics", Times.Never(), "");
    }
    
    [Test]
    public async Task PublishAsync_ShouldLog_WhenGettingStatisticsThrows()
    {
        Hangfire.JobStorage.Current = new JobStorageStub(() => new MonitoringApiStub(() => throw new Exception()));
        
        await _statisticsPublisher!.PublishAsync(CancellationToken.None);
        
        _loggerMock!.VerifyLog(LogLevel.Warning, "Hangfire Monitoring API failed to return statistics. Statistics will not be published", Times.Once());
    }
    
    [Test]
    public async Task PublishAsync_ShouldNotSendStatistics_WhenGettingStatisticsThrows()
    {
        Hangfire.JobStorage.Current = new JobStorageStub(() => new MonitoringApiStub(() => throw new Exception()));
        
        await _statisticsPublisher!.PublishAsync(CancellationToken.None);
        
        _handler.VerifyRequest(HttpMethod.Post, "https://test/api/statistics", Times.Never(), "");
    }
    
    [Test]
    public async Task PublishAsync_ShouldLog_WhenPostingStatisticsThrows()
    {
        Hangfire.JobStorage.Current = new JobStorageStub(() => new MonitoringApiStub(() => _statistics));
        _handler.SetupRequest(HttpMethod.Post, "https://test/api/statistics").Throws(new Exception());
        
        await _statisticsPublisher!.PublishAsync(CancellationToken.None);
        
        _loggerMock!.VerifyLog(LogLevel.Warning, "Failed to post statistics.", Times.Once());
    }
    
    [Test]
    public async Task PublishAsync_ShouldLog_WhenPostingStatisticsReceivesStatusCodeDifferentFromCreated()
    {
        Hangfire.JobStorage.Current = new JobStorageStub(() => new MonitoringApiStub(() => _statistics));
        _handler.SetupRequest(HttpMethod.Post, "https://test/api/statistics").ReturnsResponse(HttpStatusCode.InternalServerError);
        
        await _statisticsPublisher!.PublishAsync(CancellationToken.None);
        
        _loggerMock!.VerifyLog(LogLevel.Warning, "Failed to post statistics. Status code: InternalServerError", Times.Once());
    }
}