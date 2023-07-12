using System;
using System.Collections.Generic;
using System.IO;
using Buildables;
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
using Zenject;

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


        private ReactiveProperty<IEditableTile> _editableTile;
        private PathBuilder _pathBuilder;
        private bool _isValid;
        private string _errorMessage;
        private EditableTile _tile;
        private PathBuilder PathBuilder => _pathBuilder ??= new PathBuilder(this);

        private bool _isStillBuilding = false;
        private IDisposable _currentBuildAction;
        private ToolContext context;
       [Inject] private void Install(ToolContext context)
        {
            this.context = context;
        }
        

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

        public override bool CanBeUsedOutsideBuilding() => false;

        public override void SetPreviewVisible(bool visible)
        {
            PathBuilder.SetPreviewVisible(visible);
        }


        public override bool IsPlacementInvalid(ref string errorMessage)
        {
            if (this.Building.IsCellOverlappingMachine(AimHandler.HoveredPosition.Value))
            {
                errorMessage = "Cannot build on top of a machine";
                return true;
            }
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
            
        }


        public override bool ToolUsesModes(out List<string> modes)
        {
            // modes = _modeNames;
            modes = null;
            return false;
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
                if (_isValid)
                {
                    var tile = _tile;
                    var cell = AimHandler.HoveredPosition.Value;
                    building.Map.SetTile(cell, tile);
                }
            }
        }


    }
}