using Cysharp.Threading.Tasks;
using Interactions;
using UnityEngine;

namespace Buildings.Appliances
{
    [RequireComponent(typeof(Sink))]
    public class SinkInteraction : Interactable
    {
        private Sink _sink;
        private Sink sink => _sink ? _sink : _sink = GetComponent<Sink>();
        
        

        public override UniTask<bool> Interact(InteractionAgent agent)
        {
            sink.IsOn = !sink.IsOn;
            return UniTask.FromResult(true);
        }
    }
}