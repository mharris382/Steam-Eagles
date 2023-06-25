using System.Collections.Generic;
using Buildings;
using CoreLib;
using Items;
using UnityEngine;
using ToolControllerBase = Tools.BuildTool.ToolControllerBase;

namespace Tools.GenericTools
{
    public class NullTool : ToolControllerBase
    {
        public override bool CanBeUsedOutsideBuilding()
        {
            return true;
        }

        public override ToolStates GetToolState()
        {
            return ToolStates.HAND_LEFT;
        }

        public override BuildingLayers GetTargetLayer()
        {
            return BuildingLayers.NONE;
        }

        public override bool UsesRecipes(out List<Recipe> recipes)
        {
            recipes = null;
            return false;
        }
    }
}