using System;
using System.Collections;
using System.Collections.Generic;
using Buildings;
using JetBrains.Annotations;
using QuikGraph;
using UniRx;
using UnityEngine;

namespace Power.Steam
{
    public abstract class NetworkController<T> : MonoBehaviour where T : NetworkController<T>
    {
        public static T CreateControllerForBuilding(Building building) 
        {
            if (building.gameObject.TryGetComponent<T>(out var controller))
            {
                return controller;
            }
            return building.gameObject.AddComponent<T>();
        }
        
        
    }
    
    public abstract class NetworkController<T, TVertex, TEdge> : NetworkController<T> 
        where T : NetworkController<T, TVertex, TEdge> 
        where TVertex : NetworkNode 
        where TEdge : IEdge<TVertex>
    {
        
        
        public INetwork<TVertex, TEdge> SteamNetwork { get; private set; }
        private Subject<Unit> onNetworkUpdated = new Subject<Unit>();
        private Coroutine _networkUpdateCoroutine;
        
        
        public IObservable<Unit> OnNetworkUpdated => onNetworkUpdated;

        public void AssignNetwork(INetwork<TVertex, TEdge> network)
        {
            SteamNetwork = network;
            OnNetworkInitialized();
            if(_networkUpdateCoroutine != null)
                StopCoroutine(_networkUpdateCoroutine);
            _networkUpdateCoroutine = StartCoroutine(DoNetworkUpdates());
        }

        private IEnumerator DoNetworkUpdates()
        {
            while (true)
            {
                float timeLoopStarted = Time.time;
                var targetTime = timeLoopStarted + GetUpdateInterval();
                
                foreach (var graphVertex in SteamNetwork.Graph.Vertices)
                {
                    if(graphVertex is INetworkUpdatable updatable)
                        updatable.UpdateNetwork();
                }

                yield return UpdateNetwork();
                NetworkUpdated();
                
                yield return new WaitForSeconds(Mathf.Max(0, targetTime - Time.time));
            }
        }

        protected virtual void OnNetworkInitialized() { }

        protected virtual void NetworkUpdated()
        {
            onNetworkUpdated.OnNext(Unit.Default);
            SteamNetwork.NetworkUpdated();
        }

        public abstract float GetUpdateInterval();
        
        public abstract IEnumerator UpdateNetwork();
    }

    public interface INetwork<TVertex, TEdge> where TVertex : NetworkNode where TEdge : IEdge<TVertex>
    {
        public AdjacencyGraph<TVertex, TEdge> Graph { get; }
        public IEnumerable<TVertex> GetSupplierNodes();
        public IEnumerable<TVertex> GetConsumerNodes();

        void NetworkUpdated();
    }
    public interface INetwork<TVertex, TEdge, out TConsumer, out TSupplier>
        where TVertex : NetworkNode 
        where TEdge : IEdge<TVertex>
        where TConsumer : NetworkNode, INetworkConsumer
        where TSupplier : NetworkNode, INetworkSupplier
    {
        public AdjacencyGraph<TVertex, TEdge> Graph { get; }
        public IEnumerable<TSupplier> GetSupplierNodes();
        public IEnumerable<TConsumer> GetConsumerNodes();
    }
}