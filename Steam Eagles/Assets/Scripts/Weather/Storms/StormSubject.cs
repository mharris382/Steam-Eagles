using CoreLib;
using CoreLib.Entities;
using Zenject;

namespace Weather.Storms
{
    public class StormSubject : ISlowTickable
    {
        public class Factory : PlaceholderFactory<IStormSubject, StormSubject> { }
        
        private readonly IStormSubject _stormSubject;
        
        public EntityType SubjectEntityType => _stormSubject.SubjectEntityType;

        public StormSubject(IStormSubject stormSubject)
        {
            _stormSubject = stormSubject;
        }

        public void SlowTick(float deltaTime)
        {
            
        }
    }
}