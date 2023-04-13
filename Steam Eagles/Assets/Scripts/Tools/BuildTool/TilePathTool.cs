using System;
using System.Collections;
using System.Collections.Generic;
using Buildings;
using Buildings.Tiles;
using Characters;
using UniRx;
using UnityEngine;

namespace Tools.BuildTool
{
    [RequireComponent(typeof(TilePathPreviewerGUI))]
    public class TilePathTool : MonoBehaviour
    {
        public int maxPathLength = 10;
        public EditableTile tile;
        private TilePathPreviewerGUI _previewer;
        private ToolState _toolState;
        private Building _building;
        public PlacementMode mode = PlacementMode.ShortestPath;
        
        public enum PlacementMode
        {
            ShortestPath,
            Line,
        }

        private Building Building => _building != null ? _building : (_building = GetComponentInParent<Building>());
        private TilePathPreviewerGUI Previewer => _previewer ??= GetComponent<TilePathPreviewerGUI>();


        private List<Vector3Int> _path = new List<Vector3Int>();

        private Subject<List<Vector3Int>> _pathSubject = new Subject<List<Vector3Int>>();
        private ReactiveProperty<IEditableTile> _editableTile;
        private ReactiveProperty<Vector3Int> _hoveredPosition = new ReactiveProperty<Vector3Int>();
        private Vector3Int? _pathStart;

        public Vector3Int HoveredPosition => !HasResources()
            ? Vector3Int.zero
            : Building.Map.WorldToCell(_toolState.AimPositionWorld, Tile.GetLayer());

        public IEditableTile Tile => _editableTile.Value;

        private void Awake()
        {
            _toolState = GetComponentInParent<ToolState>();
            _editableTile = new ReactiveProperty<IEditableTile>(tile);
            Debug.Assert(_toolState != null, "No Tool State found on parent", this);
            _hoveredPosition = new ReactiveProperty<Vector3Int>();

        }

        IEnumerator Start()
        {
            while (!HasResources())
            {
                yield return null;
            }

            Previewer.Setup(_hoveredPosition, _editableTile, _pathSubject, _building.Map, _building);
            _hoveredPosition.Subscribe(UpdatePath).AddTo(this);
        }

        private void Update()
        {
            if (!HasResources())
                return;
            if (_toolState.Inputs.UsePressed) UsePressed();
            if (_toolState.Inputs.CancelPressed) _pathStart = null;
            _hoveredPosition.Value = HoveredPosition;
        }

        private void UsePressed()
        {
            if (!_pathStart.HasValue)
                _pathStart = HoveredPosition;
            else
            {
                //TODO: check if path if valid, if so build
                bool allValid = true;
                foreach (var cell in _path)
                {
                    if (!tile.IsPlacementValid(cell, _building.Map))
                    {
                        allValid = false;
                        break;
                    }
                }

                if (allValid)
                {
                    foreach (var cell in _path)
                    {
                        _building.Map.SetTile(cell, tile.GetLayer(), tile);
                    }
                }
                _pathStart = null;
            }
        }


        private void OnEnable()
        {
            Previewer.enabled = true;
        }

        private void OnDisable()
        {
            Previewer.enabled = false;
        }

        void UpdatePath(Vector3Int hoveredPosition)
        {
            if (_pathStart == null)
            {
                if (_path.Count == 1)
                    _path[0] = hoveredPosition;
                else
                    _path = new List<Vector3Int> { hoveredPosition };
                _pathSubject.OnNext(_path);
            }
            else
            {
                var current = _pathStart.Value;
                _path = new List<Vector3Int> { current };
                BuildPathShortest(hoveredPosition, current);
                _pathSubject.OnNext(_path);
            }
        }

        private void BuildPathLine(Vector3Int end, Vector3Int start)
        {
            
        }
        
        private void BuildPathShortest(Vector3Int hoveredPosition, Vector3Int current)
        {
            if (_pathStart == null)
                return;
            if (_path == null) _path = new List<Vector3Int>(maxPathLength);
            _path.Clear();
            var start = (Vector2Int) _pathStart.Value;
            var end =(Vector2Int) hoveredPosition;
            var path = _building.Map.GetPath(start, end, Tile.GetLayer());
            
            
            int length = 0;
            foreach (var position in path)
            {
                if (length >= maxPathLength)
                    break;
                _path.Add(position);
                length++;
            }
            _pathSubject.OnNext(_path);
            // Vector3Int direction = Vector3Int.zero;
            //
            // if (start.x != end.x) direction.x = start.x > end.x ? -1 : 1;
            // if(start.y != end.y) direction.y = start.y > end.y ? -1 : 1;
            //
            // int length = 0;
            // while (current != hoveredPosition && length < maxPathLength)
            // {
            //     current += direction;
            //     length++;
            //     _path.Add(current);
            //     if (current.x == hoveredPosition.x) direction.x = 0;
            //     if (current.y == hoveredPosition.y) direction.y = 0;
            // }
        }


        private bool HasResources() =>Building != null && Building.Map != null && _toolState != null && Tile != null;
    }

}