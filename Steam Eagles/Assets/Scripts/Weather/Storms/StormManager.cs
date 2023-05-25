using System.Collections.Generic;
using System.Linq;
using CoreLib;
using UniRx;


namespace Weather.Storms
{
    public class StormManager : IExtraSlowTickable
    {
        private readonly GlobalStormConfig _config;
        private readonly StormRegistry _stormRegistry;
        private readonly StormSubjectsRegistry _subjectsRegistry;

        private readonly Storm.Factory _stormFactory;
        private Queue<StormCreationRequest> _creationRequests = new Queue<StormCreationRequest>();
        
    
        public StormManager(GlobalStormConfig config,
            StormRegistry stormRegistry,
            StormSubjectsRegistry subjectsRegistry,
            Storm.Factory stormFactory)
        {
            _config = config;
            _stormRegistry = stormRegistry;
            _subjectsRegistry = subjectsRegistry;
            _stormFactory = stormFactory;
            MessageBroker.Default.Receive<StormCreationRequest>()
                .Subscribe(request => _creationRequests.Enqueue(request));
        }


        public void ExtraSlowTick(float deltaTime)
        {
            CompleteStormJobs();
            CreateStorms();
            _config.Log($"StormManager ExtraSlowTick:\n Active Storm Count:{_stormRegistry.Count}");
            RunStormJobs();
        }

        private void CreateStorms()
        {
            while (_creationRequests.Count > 0)
            {
                
                var request = _creationRequests.Dequeue();
                var storm = _stormFactory.Create(request.StormBounds, request.StormVelocity, request.StormFalloff);
                
                if (!string.IsNullOrEmpty(request.StormTag))
                    _stormRegistry.AddStorm(storm, request.StormTag);
                else
                    _stormRegistry.AddStorm(storm);
            }
        }

        private void CompleteStormJobs()
        {
            
        }

        private void RunStormJobs()
        {
            
        }
    }
}