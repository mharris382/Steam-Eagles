using Buildings;
using Power.Steam;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Buildables
{
    public class PipeInputCell : PipeCell<ConsumerNode>
    {
        public override Color GizmoColor => new Color(0.1f, 0.8f, 0.4f, 0.5f);
        
        public UnityEvent<float> onGasInput = new UnityEvent<float>();

        protected override ConsumerNode GetSteamNode(Vector3Int cell, SteamNetworks.SteamNetwork network)
        {
            var consumer =network.AddConsumerAt(cell);
            consumer.MaxConsumptionPerUpdate = gasTargetAmount;
            
            AddDisposable(
                consumer.OnConsumed.Subscribe(OnGasInput),
                Disposable.Create(() => network.RemoveConsumerAt(cell))
                );
            
            return consumer;
        }
        
        protected override void OnNodeUpdate(ConsumerNode node)
        {
            node.MaxConsumptionPerUpdate = gasTargetAmount;
            Log($"Updating consumer node {name} to {gasTargetAmount}");
        }
        
        
        protected virtual void OnGasInput(float amount)
        {
            Log($"{BuildableMachineBase.name} just consumed {amount} gas from {name}");
            onGasInput?.Invoke(amount);
        }
    }
}