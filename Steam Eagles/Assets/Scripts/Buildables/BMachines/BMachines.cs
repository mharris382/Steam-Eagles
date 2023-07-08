using System;
using System.Collections.Generic;
using Buildings;
using UniRx;
using UnityEngine;
using Zenject;

namespace Buildables
{
    public class BMachines : MonoBehaviour
    {
        private BMachineMap _bMachineMap;
        private Dictionary<BuildableMachineBase, Vector2Int> _machineInstances = new();


        private Building _building;
        public Building Building => _building ??= GetComponent<Building>();
        public BMachineMap Map => _bMachineMap;
        [Inject] void InjectMe(BMachineMap bMachineMap)
        {
            _bMachineMap = bMachineMap;
        }

        public bool HasResources() => _bMachineMap != null;


        public BuildableMachineBase Build(BuildableMachineBase prefab, Vector2Int position, bool isFlipped)
        {
            prefab.IsFlipped = isFlipped;
            var machine = prefab.Build((Vector3Int)position, Building);
            _bMachineMap.PlaceMachine(machine, position);
            _machineInstances.TryAdd(machine, position);
            return machine;
        }
        
        public void RemoveMachine(BuildableMachineBase machineBase)
        {
            _bMachineMap.RemoveMachineAt(machineBase.CellPosition);
        }
    }
}