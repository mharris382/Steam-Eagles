using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Buildings;
using Buildings.Messages;
using Buildings.Rooms;
using Buildings.Rooms.Tracking;
using Buildings.Tiles;
using CoreLib;
using Items;
using Tools.RecipeTool;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tools.BuildTool
{
    public enum ToolButtonEvent
    {
        NONE,
        CONFIRM,
        CANCEL
    }

    public class BuildActionHandle
    {
        private readonly MonoBehaviour _caller;
        private readonly float _duration;
        private readonly int _intervals;
        public bool IsActionComplete { get;  private set; }
        public bool IsStarted { get; private set; }
        public event Action<int> onBuildActionInterval;
        public event Action onBuildActionCompleted;

        public BuildActionHandle(MonoBehaviour caller, float duration = 0.2f, int intervals = 3)
        {
            _caller = caller;
            _duration = duration;
            _intervals = intervals;
            IsActionComplete = false;
            IsStarted = false;
            onBuildActionCompleted += () => IsActionComplete = true;
        }

        public void StartAction()
        {
            if (IsStarted)
            {
                Debug.LogError("BuildActionHandle: Action already started!");
                return;
            }
            IsStarted = true;
            _caller.StartCoroutine(DoBuildAction(_duration, _intervals,
                (t) => onBuildActionInterval?.Invoke(t), 
                () => onBuildActionCompleted?.Invoke())
            );
        }

        IEnumerator DoBuildAction(float duration, int intervals, Action<int> onInterval, Action onBuildCompleted)
        {
            float durationPerInterval = duration / intervals;
            for (int i = 0; i < intervals; i++)
            {
                onInterval?.Invoke(i);
                yield return new WaitForSeconds(durationPerInterval);
            }
        }
    }
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
    
    
    
    public class BuildToolController : RecipeToolBase<TileBase>
    {
        public bool allowPartiallyValidPath = true;
        public Tool defaultBuildTool;
        private StringReactiveProperty _toolMode = new StringReactiveProperty();
        public TilePathPreviewerGUI gui;

        public PathBuilderConfig config;
        
        private Dictionary<string, BuildToolMode> _buildModes = new Dictionary<string, BuildToolMode>();
        private List<BuildToolMode> _modes = new List<BuildToolMode>();
        private List<string> _modeNames = new List<string>();

        private ReactiveProperty<IEditableTile> _editableTile;
        private PathBuilder _pathBuilder;
        private bool _isValid;
        private string _errorMessage;
        private EditableTile _tile;
        private PathBuilder PathBuilder => _pathBuilder ??= new PathBuilder(this, this.gui);


        public override string ToolMode
        {
            get => _toolMode.Value;
            set => _toolMode.Value = value;
        }

        protected override void OnRoomChanged(Room room)
        {
            if (room == null)
            {
                //pathTool.enabled = false;
                return;
            }
            //pathTool.enabled = room.buildLevel == BuildLevel.FULL;
            //HasRoom = room.buildLevel == BuildLevel.FULL;
        }

        public override void SetPreviewVisible(bool visible)
        {
            PathBuilder.SetPreviewVisible(visible);
        }


        public override bool IsPlacementInvalid(ref string errorMessage)
        {
            if (!_isValid)
            {
                errorMessage = _errorMessage;
                return true;
            }
            return false;
        }

        protected override IEnumerable<Recipe> GetRecipes() => tool.Recipes;

        public override ToolStates GetToolState() => ToolStates.Build;


        protected override void OnAwake()
        { 
            _modes.Add(new LineBuildMode());
            _modes.Add(new PathBuildMode());
            foreach (var buildToolMode in _modes)
            {
                _modeNames.Add(buildToolMode.ModeName);
                _buildModes.Add(buildToolMode.ModeName, buildToolMode);
            }
            _toolMode.Value = _modeNames[0];
            _toolMode.Select(t=> _buildModes[t]).Subscribe(t => {
                //pathTool.SetStrategy(t);
                PathBuilder.PathStrategy = t;
            }).AddTo(this);
        }

        public override BuildingLayers GetTargetLayer()
        {
            if (_tile == null)
            {
                return BuildingLayers.SOLID;
            }
            return _tile.GetLayer();
        }


        protected override bool ToolUsesModes(out List<string> modes)
        {
            modes = _modeNames;
            return true;
        }

        public override void UpdatePreview(Building building, bool isFlipped,
            TileBase previewResource)
        {
            _tile = previewResource as EditableTile;
            var position = AimHandler.HoveredPosition.Value;
            PathBuilder.Update(building, _tile, ref position, ref _isValid, ref _errorMessage);
        }

        public override void OnToolUnEquipped()
        {
            if (_currentBuildAction != null)
            {
                _currentBuildAction.Dispose();
                _currentBuildAction = null;
            }
            base.OnToolUnEquipped();
        }

        protected override void OnUpdate(Building building, bool isFlipped)
        {
            if (_tile == null)
            {
                return;
            }
            

            if (ToolState.Inputs.CancelPressed )
            {
                PathBuilder.OnCancel();
            }

            if (ToolState.Inputs.UsePressed && !_isStillBuilding)
            {
                if (PathBuilder.HasFirstPoint && PathBuilder.HasValidPath)
                {
                    var buildAction = PathBuilder.GetBuildPathAction(out var buildSubject);
                    var buildTile = _editableTile.Value;
                    _currentBuildAction = buildSubject.Subscribe(
                        cell => building.Map.SetTile(cell, buildTile.GetLayer(), buildTile as EditableTile),
                        er => _isStillBuilding = false,
                        () => _isStillBuilding = false);
                    buildAction.StartAction();
                    _isStillBuilding = true;
                }
                else
                {
                    PathBuilder.SetFirstPoint(AimHandler.HoveredPosition.Value);
                }
            }
        }

        private bool _isStillBuilding = false;
        private IDisposable _currentBuildAction;
    }

    public class LineBuildMode : BuildToolMode
    {
        public override string ModeName => "Line";
        public override bool BuildPath(BuildPathInfo info, ref List<Vector3Int> path)
        {
            if (path == null)
            {
                path = new List<Vector3Int>();
            }
            var difference = info.end - info.start;
            var slope = difference.y / (difference.x != 0 ? difference.x : 1);
            var yIntercept = info.start.y - slope * info.start.x;
            var x = info.start.x;
            var y = info.start.y;
            while (x < info.end.x)
            {
                x++;
                y = slope * x + yIntercept;
                path.Add(new Vector3Int(x, Mathf.RoundToInt(y), 0));
            }
            return true;
        }
    }

    public class PathBuildMode : BuildToolMode
    {
        public override string ModeName => "Path";
        public override bool BuildPath(BuildPathInfo info, ref List<Vector3Int> path)
        {
            if (path == null)
            {
                path = new List<Vector3Int>();
            }
            path.Add(info.start);
            path.AddRange(info.building.Map.GetPath((Vector2Int)info.start ,(Vector2Int)info.end, info.layer));
            return true;
        }
    }

    public abstract class BuildToolMode : IBuildPathStrategy
    {
        public abstract string ModeName { get; }
        public abstract bool BuildPath(BuildPathInfo info, ref List<Vector3Int> path);
    }


    [Serializable]
    public class PathBuilderConfig
    {
        public float timePerTile = 0.1f;
        
    }
}