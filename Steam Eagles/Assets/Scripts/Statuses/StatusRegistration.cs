using System.Collections.Generic;

namespace Statuses
{
    public record StatusRegistration
    {
        public readonly string Status;
        public readonly string BlockingStatuses;
        public readonly string RequiredStatuses;

        public StatusRegistration(string status, string blockingStatuses, string requiredStatuses)
        {
            Status = status;
            BlockingStatuses = blockingStatuses;
            RequiredStatuses = requiredStatuses;
        }
        
        public IEnumerable<string> GetDefinedBlockingStatuses() => BlockingStatuses.Split(',');
        public IEnumerable<string> GetDefinedRequiredStatuses() => RequiredStatuses.Split(',');
    }
}