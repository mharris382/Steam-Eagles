using System;
using System.Collections;
using CoreLib.Entities;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Buildings.CoreData
{
    public class BuildingCoreData : IDisposable, IInitializable
    {
        private readonly Building _building;
        private readonly EntityInitializer _entityInitializer;
        private readonly CoroutineCaller _coroutineCaller;
        private Coroutine _initializeCoroutine;

        public BuildingCoreData(Building building, CoroutineCaller coroutineCaller)
        {
            _building = building;
            _entityInitializer = building.GetComponentInChildren<EntityInitializer>();
            _coroutineCaller = coroutineCaller;
        }
        public void Dispose()
        {
            Debug.Log($"BuildingCoreData Disposed for {_building.name}",_building);
            if (_initializeCoroutine != null) _coroutineCaller.StopCoroutine(_initializeCoroutine);
        }
        public void Initialize()
        {
            Debug.Log($"BuildingCoreData Initialized for {_building.name}",_building);
            _initializeCoroutine = _coroutineCaller.StartCoroutine(WaitForBuildingToLoad());
        }

        private IEnumerator WaitForBuildingToLoad()
        {
            while (!_entityInitializer.isDoneInitializing)
            {
                Debug.Log($"BuildingCoreData waiting for building {_building.name} to load",_building);
                yield return null;
            }

            // yield return UniTask.ToCoroutine(async () =>
            // {
            //     
            // });
            InitializeBuildingCoreData();
            _initializeCoroutine = null;
        }

        void InitializeBuildingCoreData()
        {
            var map = _building.Map;
            var graph = map.RoomGraph.Graph;
            foreach (var graphVertex in graph.Vertices)
            {
                if (graph.TryGetOutEdges(graphVertex, out var outEdges))
                {
                    
                    
                    
                }
            }
        }
    }
}