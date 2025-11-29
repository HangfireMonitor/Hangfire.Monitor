using Hangfire.Storage;

namespace Hangfire.Monitor.Core.Tests;

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