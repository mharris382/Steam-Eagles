using System.Collections.Generic;
using System.Linq;
using CoreLib;
using UniRx;


namespace Weather.Storms
{
    public class StormManager : IExtraSlowTickable
    {
        private readonly GlobalStormConfig _config;
        Queue<StormCreationRequest> _creationRequests = new Queue<StormCreationRequest>();
        Dictionary<string, Storm> _taggedStorms = new Dictionary<string, Storm>();
        private List<Storm> _activeStorms = new List<Storm>();
        public StormManager(GlobalStormConfig config)
        {
            _config = config;
            MessageBroker.Default.Receive<StormCreationRequest>()
                .Subscribe(request => _creationRequests.Enqueue(request));
        }


        public void ExtraSlowTick(float deltaTime)
        {
            _config.Log($"StormManager ExtraSlowTick:\n Active Storm Count:{_activeStorms.Count}");
        }
    }
}