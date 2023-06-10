using System;
using System.Collections.Generic;
using Buildings;
using UnityEngine;
using UniRx;
using Zenject;

namespace Buildables
{
    public class BMachineDebugger : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer cellPrefab;
        [SerializeField] private Transform parent;

        private BMachineMap _bMachineMap;
        private BMachines _bMachines;
        private Building _building;
        
        
        private Dictionary<Vector2Int, SpriteRenderer> _usedRenderers = new Dictionary<Vector2Int, SpriteRenderer>();

        [Inject] void InjectMe(BMachineMap bMachineMap)
        {
            _bMachineMap = bMachineMap;
            _bMachineMap.OnMachinePlaced.Subscribe(OnMachinePlaced).AddTo(this);
            _bMachineMap.OnMachineRemoved.Subscribe(OnMachineRemoved).AddTo(this);
        }

        void OnMachineRemoved(BMachine machine)
        {
            foreach (var machineCell in machine.Cells)
            {
                Hide(machineCell);
            }
        }
        void OnMachinePlaced(BMachine machine)
        {
            foreach (var cell in machine.Cells)
            {
                Show(cell);
            }
        }

        private void Awake()
        {
            _bMachines = GetComponent<BMachines>();
            _building = GetComponent<Building>();
        }


        private void Hide(Vector2Int position)
        {
            var sr = GetRenderer(position);
            sr.enabled = false;
        }
        private void Show(Vector2Int position)
        {
            var sr = GetRenderer(position);
            sr.enabled = true;
        }

        private SpriteRenderer GetRenderer(Vector2Int position)
        {
            if (!_usedRenderers.TryGetValue(position, out var sr))
            {
                sr = Instantiate(cellPrefab, parent);
                sr.transform.position = _building.Map.CellToWorldCentered((Vector3Int)position, BuildingLayers.SOLID);
                _usedRenderers.Add(position, sr);
            }
            return sr;
        }
    }
}