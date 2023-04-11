using CoreLib.Entities;

namespace Statuses
{
    public interface IEntityStatuses
    {
        EntityType EntityType { get; }
        StatusState GetStatusState(StatusHandle status);
        StatusState AddStatus(StatusHandle kvpValue);
    }
}