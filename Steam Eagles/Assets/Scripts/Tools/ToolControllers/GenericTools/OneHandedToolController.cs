using System;
using System.Collections.Generic;
using Buildings;
using Buildings.Rooms;
using CoreLib;
using Items;
using UnityEngine;
using ToolControllerBase = Tools.BuildTool.ToolControllerBase;

namespace Tools.GenericTools
{
    public class OneHandedToolController : ToolControllerBase
    {
        public bool isLeftHand;
        protected override void OnRoomChanged(Room room)
        {
            Debug.LogWarning($"{nameof(OneHandedToolController)} throw new System.NotImplementedException();");
        }

        public override bool CanBeUsedOutsideBuilding()
        {
            return true;
        }

        public override ToolStates GetToolState()
        {
            return isLeftHand ? ToolStates.HAND_LEFT : ToolStates.HAND_RIGHT;
        }

        public override BuildingLayers GetTargetLayer() => BuildingLayers.SOLID;

        public override bool UsesRecipes(out List<Recipe> recipes)
        {
            recipes = null;
            return false;
        }
    }
}