using System.Collections.Generic;
using UniRx;


namespace Weather.Storms
{
    public class StormManager 
    {
        Queue<StormCreationRequest> _creationRequests = new Queue<StormCreationRequest>();
        Dictionary<string, Storm> _taggedStorms = new Dictionary<string, Storm>();
        private List<Storm> _activeStorms = new List<Storm>();
        public StormManager()
        {
            MessageBroker.Default.Receive<StormCreationRequest>()
                .Subscribe(request => _creationRequests.Enqueue(request));
        }

        
    }
}