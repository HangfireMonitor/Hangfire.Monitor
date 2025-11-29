using Hangfire.Storage.Monitoring;

namespace Hangfire.Monitor.Core
{
    public class Payload
    {
        public string Name { get; set; }
        
        public long Servers { get; set; }

        public long Recurring { get; set; }

        public long Enqueued { get; set; }

        public long Queues { get; set; }

        public long Scheduled { get; set; }

        public long Processing { get; set; }

        public long Succeeded { get; set; }

        public long Failed { get; set; }

        public long Deleted { get; set; }

        public static Payload Create(string name, StatisticsDto statistics)
        {
            return new Payload
            {
                Name = name,
                Servers = statistics.Servers,
                Recurring = statistics.Recurring,
                Enqueued = statistics.Enqueued,
                Queues = statistics.Queues,
                Scheduled = statistics.Scheduled,
                Processing = statistics.Processing,
                Succeeded = statistics.Succeeded,
                Failed = statistics.Failed,
                Deleted = statistics.Deleted
            };
        }
    }
}