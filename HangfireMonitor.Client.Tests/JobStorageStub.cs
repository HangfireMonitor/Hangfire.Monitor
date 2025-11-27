using Hangfire;
using Hangfire.Storage;

namespace HangfireMonitor.Client.Tests;

public class JobStorageStub(Func<IMonitoringApi?> getMonitoringApi) : JobStorage
{
    public override IMonitoringApi GetMonitoringApi()
    {
        return getMonitoringApi();
    }

    public override IStorageConnection GetConnection()
    {
        throw new NotImplementedException();
    }
}