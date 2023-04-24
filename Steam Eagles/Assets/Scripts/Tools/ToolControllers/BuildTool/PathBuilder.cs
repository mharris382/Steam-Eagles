using System;
using System.Collections.Generic;
using Buildings;
using Buildings.Tiles;
using UniRx;
using UnityEngine;

namespace Tools.BuildTool
{
    public class PathBuilder : IDisposable
    {
        private IDisposable _disposable;
        private readonly BuildToolController _toolController;
        private readonly TilePathPreviewerGUI _previewerGUI;

        private readonly ReactiveProperty<Vector3Int?> _pathStart;
        private readonly ReactiveProperty<Vector3Int> _hoveredPosition;
        
        private List<Vector3Int> _path;
        private Subject<List<Vector3Int>> _pathSubject;
        private readonly ReactiveProperty<IEditableTile> _selectedTile;
        private readonly ReactiveProperty<Building> _building;


        public IBuildPathStrategy PathStrategy { get; set; }
        public bool HasFirstPoint => _pathStart.Value != null;

        IEditableTile Tile
        {
            get => _selectedTile.Value;
            set => _selectedTile.Value = value;
        }
        
        Building Building
        {
            get => _building.Value;
            set => _building.Value = value;
        }

        public bool HasValidPath => IsPathValid(_path);

        public void SetFirstPoint(Vector3Int firstPoint) => _pathStart.Value = firstPoint;
        public void ClearFirstPoint() => _pathStart.Value = null;
        
        
        
        public PathBuilder(BuildToolController toolController, TilePathPreviewerGUI previewerGUI)
        {
            _path = new List<Vector3Int>();
            _toolController = toolController;
            _previewerGUI = previewerGUI;
            _pathSubject = new Subject<List<Vector3Int>>();
            _pathStart = new ReactiveProperty<Vector3Int?>();
            _hoveredPosition = new ReactiveProperty<Vector3Int>();
            _selectedTile = new ReactiveProperty<IEditableTile>();
            _building = new ReactiveProperty<Building>();
            _previewerGUI.Setup(_hoveredPosition, _selectedTile, _pathSubject, _building);
        }


        public void Update(Building building, IEditableTile tile, ref Vector3Int position, ref bool isValid, ref string errorMessage)
        {
            if (building == null)
            {
                SetVisible(false);
                errorMessage = "Building is null";
                isValid = false;
                return;
            }
            if (tile == null)
            {
                SetVisible(false);
                errorMessage = "Tile is null";
                isValid = false;
                return;
            }

            Building = building;
            Tile = tile;

            var layer = tile.GetLayer();
            _previewerGUI.MoveSpriteTo(building.Map.CellToWorld(position, layer));
            if (!HasFirstPoint)
            {
                var room = building.Map.GetRoom(position, layer);
                if (room == null)
                {
                    errorMessage = "Position is not in a room";
                    isValid = false;
                    return;
                }
                SetVisible(true);
                if (!tile.CanTileBePlacedInRoom(room))
                {
                    errorMessage = $"{tile.name} cannot be placed in {room.name}";
                    isValid = false;
                    return;
                }
                isValid = true;
                errorMessage = "";
            }
            else
            {
                isValid = true;
                errorMessage = "";
                if (UpdatePath(ref position, ref _path))
                {
                    _pathSubject.OnNext(_path);
                }
                else
                {
                    _pathSubject.OnNext(null);
                }
            }
            
        }

        private bool UpdatePath( ref Vector3Int selectedPoint, ref List<Vector3Int> path, bool clearPath = true)
        {
            if(_pathStart.Value.HasValue==false)
            {
                if(clearPath) path.Clear();
                return false;
            }

            var startPoint = _pathStart.Value.Value;
            if (clearPath)
            {
                path.Clear();
                path.Add(startPoint);
            }
            PathStrategy ??= new TilePathTool.DefaultPathStrategy();
            return PathStrategy.BuildPath(new BuildPathInfo()
            {
                building = Building,
                start = startPoint,
                end = selectedPoint,
                layer = Tile.GetLayer()
            }, ref path);
        }
        
        private bool IsPathValid(List<Vector3Int> path)
        {
            bool allValid = true;
            bool anyValid = false;
            foreach (var pos in path)
            {
                if (!Tile.IsPlacementValid(pos, Building.Map))
                {
                    allValid = false;
                }
                else
                {
                    anyValid = true;
                }
            }
            return _toolController.allowPartiallyValidPath ? anyValid : allValid;
        }

 
        private void SetVisible(bool visible)
        {
            _previewerGUI.enabled = visible;
        }

        public void Dispose()
        {
            _disposable?.Dispose();
            _pathStart?.Dispose();
            _hoveredPosition?.Dispose();
            _pathSubject?.Dispose();
            _selectedTile?.Dispose();
            _building?.Dispose();
        }

        public void OnCancel()
        {
            if (HasFirstPoint) ClearFirstPoint();
        }

        public BuildActionHandle GetBuildPathAction(out Subject<Vector3Int> onPathPointBuilt)
        {
            var buildHandle = new BuildActionHandle(_toolController, duration: _toolController.config.timePerTile * _path.Count,
                _path.Count);
            var subject = new Subject<Vector3Int>();
            buildHandle.onBuildActionCompleted += () => subject.OnCompleted();
            buildHandle.onBuildActionInterval += i => subject.OnNext(_path[i]);
            onPathPointBuilt = subject;
            return buildHandle;
        }

        public void SetPreviewVisible(bool visible)
        {
            _previewerGUI.enabled = visible;
            ClearFirstPoint();
            _path.Clear();
        }
    }
}