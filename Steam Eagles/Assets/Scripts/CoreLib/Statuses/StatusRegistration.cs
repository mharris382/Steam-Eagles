using System.Collections.Generic;
using CoreLib.MyEntities;

namespace Statuses
{
    public record StatusRegistration
    {
        public readonly string Status;
        public readonly string BlockingStatuses;
        public readonly string RequiredStatuses;
        public readonly EntityType targetEntityMask;

        public StatusRegistration(string status, string blockingStatuses, string requiredStatuses, EntityType targetEntityMask, EntityType entityMask = EntityType.ALL)
        {
            Status = status;
            BlockingStatuses = blockingStatuses;
            RequiredStatuses = requiredStatuses;
            this.targetEntityMask = targetEntityMask;
        }
        
        public IEnumerable<string> GetDefinedBlockingStatuses() => BlockingStatuses.Split(',');
        public IEnumerable<string> GetDefinedRequiredStatuses() => RequiredStatuses.Split(',');
    }
}