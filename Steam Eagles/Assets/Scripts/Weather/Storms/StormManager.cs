using System.Collections.Generic;
using System.Linq;
using CoreLib;
using UniRx;


namespace Weather.Storms
{
    public class StormManager : IExtraSlowTickable
    {
        private readonly GlobalStormConfig _config;
        private readonly Storm.Factory _stormFactory;
        Queue<StormCreationRequest> _creationRequests = new Queue<StormCreationRequest>();
        Dictionary<string, Storm> _taggedStorms = new Dictionary<string, Storm>();
        private List<Storm> _activeStorms = new List<Storm>();
        public StormManager(GlobalStormConfig config, Storm.Factory stormFactory)
        {
            _config = config;
            _stormFactory = stormFactory;
            MessageBroker.Default.Receive<StormCreationRequest>()
                .Subscribe(request => _creationRequests.Enqueue(request));
        }


        public void ExtraSlowTick(float deltaTime)
        {
            CompleteStormJobs();
            while (_creationRequests.Count > 0)
            {
                var request = _creationRequests.Dequeue();
                var storm = _stormFactory.Create(request.StormBounds, request.StormVelocity, request.StormFalloff);
                _taggedStorms.Add(request.StormTag, storm);
                _activeStorms.Add(storm);
            }
            _config.Log($"StormManager ExtraSlowTick:\n Active Storm Count:{_activeStorms.Count}");
            RunStormJobs();
        }

        private void CompleteStormJobs()
        {
            
        }

        private void RunStormJobs()
        {
            
        }
    }
}