﻿using Buildings;
using Power.Steam;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Buildables
{
    public class PipeOutputCell : PipeCell<SupplierNode>
    {
        public override Color GizmoColor => new Color(0.8f, 0.1f, 0.4f, 0.5f);
        public UnityEvent<float> onGasOutput = new UnityEvent<float>();

        protected override SupplierNode GetSteamNode(Vector3Int cell, SteamNetworks.SteamNetwork network)
        {
            var supplier = network.AddSupplierAt(cell);
            supplier.AmountSuppliedPerUpdate = gasTargetAmount;
            
            AddDisposable(
                supplier.OnSupplied.Subscribe(OnGasOutput),
                Disposable.Create(() => network.RemoveSupplierAt(cell))
                );
            
            return supplier;
        }
        protected override void OnNodeUpdate(SupplierNode node)
        {
            node.AmountSuppliedPerUpdate = gasTargetAmount;
            Log($"Updating supplier node {name} to {gasTargetAmount}");
        }

        protected virtual void OnGasOutput(float amount)
        {
            Log($"{BuildableMachineBase.name} just produced {amount} gas from {name}");
            onGasOutput?.Invoke(amount);
        }
    }
}