using System;
using System.Collections;
using System.Collections.Generic;
using Buildings;
using Buildings.Messages;
using Buildings.Rooms;
using Buildings.Rooms.Tracking;
using CoreLib;
using Items;
using UniRx;
using UnityEngine;

namespace Tools.BuildTool
{
    public class BuildToolController : RecipeToolBase
    {
        public TilePathTool pathTool;
        public Tool defaultBuildTool;

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

        public override void SetPreviewVisible(bool visible)
        {
            pathTool.enabled = visible;
        }

        protected override IEnumerable<Recipe> GetRecipes() => tool.Recipes;

        public override ToolStates GetToolState() => ToolStates.Build;
        
    }
}