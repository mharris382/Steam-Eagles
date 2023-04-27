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
        
        
        public INetwork<TVertex, TEdge> Network { get; private set; }
        public void AssignNetwork(INetwork<TVertex, TEdge> network)
        {
            Network = network;
            StartCoroutine(DoNetworkUpdates());
        }

        private Subject<Unit> onNetworkUpdated = new Subject<Unit>();
        public IObservable<Unit> OnNetworkUpdated => onNetworkUpdated;
        IEnumerator DoNetworkUpdates()
        {
            while (true)
            {
                yield return new WaitForSeconds(GetUpdateInterval());
                foreach (var node in Network.GetConsumerNodes())
                {
                    var consumer = node as INetworkConsumer;
                    consumer.UpdateNetwork();
                }
                foreach (var supplierNode in Network.GetSupplierNodes())
                {
                    var supplier = supplierNode as INetworkSupplier;
                    supplier.UpdateNetwork();
                }
                UpdateNetwork();
                onNetworkUpdated.OnNext(Unit.Default);
            }
        }

        public abstract float GetUpdateInterval();
        
        public abstract void UpdateNetwork();
    }

    public interface INetwork<TVertex, TEdge> where TVertex : NetworkNode where TEdge : IEdge<TVertex>
    {
        public AdjacencyGraph<TVertex, TEdge> Network { get; }
        public IEnumerable<TVertex> GetSupplierNodes();
        public IEnumerable<TVertex> GetConsumerNodes();
    }
    
}