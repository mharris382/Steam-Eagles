using System;
using System.Collections;
using CoreLib;
using CoreLib.MyEntities;
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
        private readonly PersistenceState _persistenceState;
        private Coroutine _initializeCoroutine;

        public BuildingCoreData(Building building, CoroutineCaller coroutineCaller, PersistenceState persistenceState)
        {
            _building = building;
            _entityInitializer = building.GetComponentInChildren<EntityInitializer>();
            _coroutineCaller = coroutineCaller;
            _persistenceState = persistenceState;
        }
        public void Dispose()
        {
            // if (_coroutineCaller == null) return;
            // Debug.Log($"BuildingCoreData Disposed for {_building.name}",_building);
            // if (_initializeCoroutine != null) _coroutineCaller.StopCoroutine(_initializeCoroutine);
        }
        public void Initialize()
        {
            Debug.Log($"BuildingCoreData Initialized for {_building.name}",_building);
            // _initializeCoroutine = _coroutineCaller.StartCoroutine(WaitForBuildingToLoad());
        }

        // private IEnumerator WaitForBuildingToLoad()
        // {
        //     yield return null;
        //     if (_persistenceState.CurrentAction != PersistenceState.Action.LOADING)
        //     {
        //         
        //         _initializeCoroutine = null;
        //         yield break;
        //     }
        //     while (!_entityInitializer.isDoneInitializing)
        //     {
        //         Debug.Log($"BuildingCoreData waiting for building {_building.name} to load",_building);
        //         yield return null;
        //     }
        //     
        //    
        //     _initializeCoroutine = null;
        // }

    }
}