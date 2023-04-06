namespace Statuses
{
    public interface IEntity
    {
        EntityType EntityType { get; }
        StatusState GetStatusState(StatusHandle status);
        StatusState AddStatus(StatusHandle kvpValue);
    }
}