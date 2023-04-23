using System;
using System.Collections;
using System.Collections.Generic;
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
    public class BuildToolController : RecipeToolBase<TileBase>
    {
        public TilePathTool pathTool;
        public Tool defaultBuildTool;
        private StringReactiveProperty _toolMode = new StringReactiveProperty();
        
        
        
        private Dictionary<string, BuildToolMode> _buildModes = new Dictionary<string, BuildToolMode>();
        private List<BuildToolMode> _modes = new List<BuildToolMode>();
        private List<string> _modeNames = new List<string>();
        
        


        public override string ToolMode
        {
            get => _toolMode.Value;
            set => _toolMode.Value = value;
        }

        protected override void OnRoomChanged(Room room)
        {
            if (room == null)
            {
                pathTool.enabled = false;
                return;
            }
            pathTool.enabled = room.buildLevel == BuildLevel.FULL;
            HasRoom = room.buildLevel == BuildLevel.FULL;
        }


        public override void SetPreviewVisible(bool visible) => pathTool.enabled = visible;

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
            _toolMode.Select(t=> _buildModes[t]).Subscribe(t=>pathTool.SetStrategy(t)).AddTo(this);
        }

        protected override bool ToolUsesModes(out List<string> modes)
        {
            modes = _modeNames;
            return true;
        }

        public override void UpdatePreview(Building building, bool isFlipped,
            TileBase previewResource)
        {
            EditableTile tile = previewResource as EditableTile;
        }

        protected override void OnRecipeChanged(Recipe recipe)
        {
            base.OnRecipeChanged(recipe);
        }

        protected override void OnUpdate(Building building, bool isFlipped)
        {
            
        }
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
}