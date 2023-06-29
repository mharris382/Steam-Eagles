using System.Collections.Generic;
using Buildings;
using Buildings.Rooms;
using CoreLib;
using Items;
using UnityEngine;
using ToolControllerBase = Tools.BuildTool.ToolControllerBase;

namespace Tools.RepairTool
{
    public class RepairToolController : ToolControllerBase
    {
        protected override void OnRoomChanged(Room room)
        {
            Debug.LogWarning($"{nameof(RepairToolController)} throw new System.NotImplementedException();");
        }

        public override bool CanBeUsedOutsideBuilding()
        {
            return true;
        }

        public override ToolStates GetToolState()
        {
            return ToolStates.Repair;
        }

        public override BuildingLayers GetTargetLayer()
        {
            return BuildingLayers.SOLID;
        }

        public override bool UsesRecipes(out List<Recipe> recipes)
        {
            throw new System.NotImplementedException();
        }
    }
}