using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Buildings
{
    public class ElectricityLineHandler : MonoBehaviour
    {
        public bool useLocalSpace = true;
        [Required] public ElecLine lineHandlerPrefab;


        private Building _building;
        private Building Building => _building != null ? _building : _building = GetComponentInParent<Building>();


        private Transform _activeLineParent;
        private Transform _inactiveLineParent;
        
        private Dictionary<BuildingCell, CellLine> _sourceToLine = new();

        private struct CellLine : IDisposable
        {
            public BuildingCell src;
            public BuildingCell dst;
            private readonly ElecLine _lineInstance;
            private readonly Transform _inactiveParent;

            public CellLine(BuildingCell src, BuildingCell dst, ElecLine lineInstance, Transform inactiveParent)
            {
                this.src = src;
                this.dst = dst;
                _lineInstance = lineInstance;
                this._inactiveParent = inactiveParent;
            }

            public (Vector2 start, Vector2 end) GetLocalSpace(Building building)
            {
                Vector2 start = building.Map.CellToLocalCentered(src);
                Vector2 end = building.Map.CellToLocalCentered(dst);
                return (start, end);
            }

            public (Vector2 start, Vector2 end) GetWorldSpace(Building building)
            {
                Vector2 start = building.Map.CellToWorldCentered(src);
                Vector2 end = building.Map.CellToWorldCentered(dst);
                return (start, end);
            }

            public void UpdateLinePoints(Building building, bool useLocalSpace)
            {
                var points = useLocalSpace ? GetLocalSpace(building) : GetWorldSpace(building);
                _lineInstance.SetPoints(points.start, points.end);
            }

            public override int GetHashCode() => HashCode.Combine(src, dst);

            public void Dispose()
            {
                _lineInstance.transform.parent = _inactiveParent;
                _lineInstance.gameObject.SetActive(false);
            }
        }


        public void RemoveLineFrom(BuildingCell src)
        {
            if (_sourceToLine.TryGetValue(src, out var line))
            {
                line.Dispose();
                _sourceToLine.Remove(src);
            }
        }

        public void AddLine(BuildingCell src, BuildingCell target)
        {
            RemoveLineFrom(src); //in case it already exists
            var newLine = Create(src, target);
            _sourceToLine.Add(src, newLine);
        }
        CellLine Create(BuildingCell src, BuildingCell target)
        {
            return new CellLine(src, target, GetLineInstance(), _inactiveLineParent);
        }

        ElecLine GetLineInstance()
        {
            if (_inactiveLineParent.childCount == 0)
            {
               var inst = Instantiate(lineHandlerPrefab, _activeLineParent);
               inst.transform.localPosition = Vector3.zero;
                return inst;
            }
            else
            {
                var line = _inactiveLineParent.GetChild(0);
                line.parent = _activeLineParent;
                line.gameObject.SetActive(true);
                return line.GetComponent<ElecLine>();
            }
        }

        [Inject] void Install(Building building)
        {
            _building = building;
        }

        private void Awake()
        {
            _activeLineParent = new GameObject("Active Electricity Lines").transform;
            _inactiveLineParent = new GameObject("Inactive Electricity Lines").transform;
            _inactiveLineParent.parent = _activeLineParent.parent = this.transform;
            _inactiveLineParent.localPosition = _activeLineParent.localPosition = Vector3.zero;
        }
    }
}