using System.Collections.Generic;
using Buildings.Rooms;
using CoreLib;
using Items;
using UnityEngine;
using ToolControllerBase = Tools.BuildTool.ToolControllerBase;

namespace Tools.RecipeTool
{
    public class CraftToolController : ToolControllerBase
    {
        protected override void OnRoomChanged(Room room)
        {
            Debug.LogWarning($"{nameof(CraftToolController)} throw new System.NotImplementedException();");
        }

        public override ToolStates GetToolState()
        {
            return ToolStates.Recipe;
        }

        public override bool UsesRecipes(out List<Recipe> recipes)
        {
            throw new System.NotImplementedException();
        }
    }
}