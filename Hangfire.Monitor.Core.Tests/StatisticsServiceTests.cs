using Moq;

namespace Hangfire.Monitor.Core.Tests;

public class StatisticsServiceTests
{
    private Mock<IStatisticsPublisher> _statisticsPublisherMock;
    private StatisticsService _statisticsService;
    private readonly TimeSpan _startupDelay = TimeSpan.FromMilliseconds(250);
    private readonly TimeSpan _interval = TimeSpan.FromMilliseconds(500);

    [SetUp]
    public void Setup()
    {
        _statisticsPublisherMock = new Mock<IStatisticsPublisher>();
        _statisticsService = new StatisticsService(_startupDelay, _interval, _statisticsPublisherMock.Object);
    }

    [Test]
    public async Task ProcessAsync_ShouldPublishStatistics()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        _statisticsPublisherMock
            .Setup(x => x.PublishAsync(It.IsAny<CancellationToken>()))
            .Callback(() => cts.Cancel())
            .Returns(Task.CompletedTask);

        try
        {
            await _statisticsService.ProcessAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected: loop cancelled either by callback or timeout
        }
       
        _statisticsPublisherMock.Verify(x => x.PublishAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Test]
    public async Task ProcessAsync_ShouldRespectStartupDelay()
    {
        using var cts = new CancellationTokenSource(_startupDelay - TimeSpan.FromMilliseconds(10));

        _statisticsPublisherMock
            .Setup(x => x.PublishAsync(It.IsAny<CancellationToken>()))
            .Callback(() => cts.Cancel())
            .Returns(Task.CompletedTask);

        try
        {
            await _statisticsService.ProcessAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected: loop cancelled either by callback or timeout
        }
       
        _statisticsPublisherMock.Verify(x => x.PublishAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Test]
    public async Task ProcessAsync_ShouldPublishStatisticsAtIntervals()
    {
        using var cts = new CancellationTokenSource( _startupDelay + _interval * 2 + TimeSpan.FromMilliseconds(10));

        _statisticsPublisherMock
            .Setup(x => x.PublishAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        try
        {
            await _statisticsService.ProcessAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected: loop cancelled either by callback or timeout
        }

        _statisticsPublisherMock.Verify(x => x.PublishAsync(It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [TearDown]
    public void TearDown()
    {
        _statisticsService.Dispose();
    }
}