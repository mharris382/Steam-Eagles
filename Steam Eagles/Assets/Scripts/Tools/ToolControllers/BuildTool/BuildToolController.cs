using System;
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
using UnityEngine.Tilemaps;

namespace Tools.BuildTool
{
    public enum ToolButtonEvent
    {
        NONE,
        CONFIRM,
        CANCEL
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
        public override BuildingLayers GetTargetLayer() => _tile == null ? BuildingLayers.SOLID : _tile.GetLayer();



        protected override void OnAwake()
        {
            SetupToolModes();
        }

        private void SetupToolModes()
        {
            
            _modes.Add(new LineBuildMode());
            _modes.Add(new PathBuildMode());
            foreach (var buildToolMode in _modes)
            {
                _modeNames.Add(buildToolMode.ModeName);
                _buildModes.Add(buildToolMode.ModeName, buildToolMode);
            }

            _toolMode.Value = _modeNames[0];
            _toolMode.Select(t => _buildModes[t]).Subscribe(t =>
            {
                //pathTool.SetStrategy(t);
                PathBuilder.PathStrategy = t;
            }).AddTo(this);
        }


        protected override bool ToolUsesModes(out List<string> modes)
        {
            modes = _modeNames;
            return true;
        }

        public override void OnToolUnEquipped()
        {
            if (_currentBuildAction != null)
            {
                _currentBuildAction.Dispose();
                _currentBuildAction = null;
            }
            SetPreviewVisible(false);
            base.OnToolUnEquipped();
        }

        public override void OnToolEquipped()
        {
            SetPreviewVisible(true);
            base.OnToolEquipped();
        }

        public override void UpdatePreview(Building building, bool isFlipped,
            TileBase previewResource)
        {
            _tile = previewResource as EditableTile;
            var position = AimHandler.HoveredPosition.Value;
            PathBuilder.Update(building, _tile, ref position, ref _isValid, ref _errorMessage);
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
}