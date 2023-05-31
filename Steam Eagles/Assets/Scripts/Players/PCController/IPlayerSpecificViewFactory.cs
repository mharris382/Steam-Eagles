using System.Collections.Generic;
using CoreLib.Interfaces;
using UnityEngine;
using Zenject;

namespace Players.PCController
{
  

    public class PCViewFactory : IFactory<int, GameObject, GameObject>, IPCViewFactory
    {
        private readonly LayerMaskFactory _layerMaskFactory;

        private Dictionary<GameObject, GameObject>[] _playerSpecificCopies;
    
        public PCViewFactory()
        {
            _layerMaskFactory = new LayerMaskFactory();
            _playerSpecificCopies = new Dictionary<GameObject, GameObject>[2]
            {
                new Dictionary<GameObject, GameObject>(),
                new Dictionary<GameObject, GameObject>()
            };
        }
        public GameObject Create(int param1, GameObject param2)
        {
            if (param2 == null)
            {
                Debug.LogError("Null value");
                return null;
            }
            var playerSpecificCopies = _playerSpecificCopies[param1];
            if (!playerSpecificCopies.TryGetValue(param2, out var copy) || copy == null)
            {
                copy = GameObject.Instantiate(param2, param2.transform.position, param2.transform.rotation,
                    param2.transform.parent);
                copy.layer = _layerMaskFactory.Create(param1);
                if (playerSpecificCopies.ContainsKey(param2)) playerSpecificCopies.Remove(param2);
                playerSpecificCopies.Add(param2, copy);
            }
            return copy;
        }
        
        public class LayerMaskFactory : IFactory<int, LayerMask>
        {
            private LayerMask[] _pcLayers;
            public LayerMaskFactory()
            {
                _pcLayers = new LayerMask[2]
                {
                    LayerMask.NameToLayer("P1"),
                    LayerMask.NameToLayer("P2")
                };
            }
            public LayerMask Create(int param) => _pcLayers[param];
        }
    }
}