namespace Statuses
{
    public interface IEntity
    {
        EntityType EntityType { get; }
        StatusState GetStatusState(string status);
    }
}