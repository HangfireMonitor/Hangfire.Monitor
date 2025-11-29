using Microsoft.Extensions.Logging;
using Moq;

namespace Hangfire.Monitor.Core.Tests;

public static class LoggerMoqExtensions
{
    public static void VerifyLog<T>(
        this Mock<ILogger<T>> mockLogger,
        LogLevel level,
        string containsMessage,
        Times? times = null)
    {
        times ??= Times.Once();

        mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(containsMessage, StringComparison.OrdinalIgnoreCase)),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            times.Value
        );
    }

    public static void VerifyLogExact<T>(
        this Mock<ILogger<T>> mockLogger,
        LogLevel level,
        string exactMessage,
        Times? times = null)
    {
        times ??= Times.Once();

        mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString() == exactMessage),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            times.Value
        );
    }
}