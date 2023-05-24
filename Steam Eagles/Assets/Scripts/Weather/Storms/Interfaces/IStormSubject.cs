using CoreLib.Entities;

namespace Weather.Storms
{
    public interface IStormSubject
    {
        public EntityType SubjectEntityType { get; }
    }
}