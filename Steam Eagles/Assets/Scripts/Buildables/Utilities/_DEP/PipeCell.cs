using System;
using System.Collections;
using Buildings;
using Power.Steam;
using UnityEngine;
using UniRx;

namespace Buildables
{
    public abstract class PipeCell : MachineCell
    {
        private SteamNode _node;
        private CompositeDisposable _disposable;
        
        
        
        [SerializeField, Tooltip("Amount that the cell tries to produce if it is an output, or consume if it is an input")] 
        protected float gasTargetAmount = 1;


        public sealed override BuildingLayers TargetLayer => BuildingLayers.PIPE;


        protected sealed override void OnCellBuilt(Vector3Int cell, Building building)
        {
            _node = GetNode(cell, building.GetSteamNetwork());
             _disposable = new CompositeDisposable();   
             _node.OnNodeUpdate.Select(_ => _node).Subscribe(OnNodeUpdate).AddTo(_disposable);
        }

        protected abstract SteamNode GetNode(Vector3Int cell, SteamNetworks.SteamNetwork network);

        protected abstract void OnNodeUpdate(SteamNode node);
    }

    public abstract class PipeCell<T> : PipeCell where T : SteamNode
    {
        public T SteamNode { get; private set; }
        protected override SteamNode GetNode(Vector3Int cell, SteamNetworks.SteamNetwork network) => SteamNode ??= GetSteamNode(cell, network);
        protected abstract T GetSteamNode(Vector3Int cell, SteamNetworks.SteamNetwork network);
        protected sealed override void OnNodeUpdate(SteamNode _) => OnNodeUpdate(SteamNode);
        protected abstract void OnNodeUpdate(T node);
    }
}