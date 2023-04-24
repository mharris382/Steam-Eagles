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
    public class TwoHandedToolController : ToolControllerBase
    {
        public OneHandedToolController leftHand;
        public OneHandedToolController rightHand;


        private void OnEnable()
        {
            leftHand.enabled = false;
            rightHand.enabled = false;
        }
        
        private void OnDisable()
        {
            leftHand.enabled = true;
            rightHand.enabled = true;
        }

        protected override void OnRoomChanged(Room room)
        {
            Debug.LogWarning($"{nameof(TwoHandedToolController)} throw new System.NotImplementedException();");
        }

        public override ToolStates GetToolState()
        {
            return ToolStates.HAND_BOTH;
        }

        public override BuildingLayers GetTargetLayer()
        {
            return BuildingLayers.SOLID;
        }

        public override bool UsesRecipes(out List<Recipe> recipes)
        {
            throw new System.NotImplementedException();
        }

        public virtual void SelectRecipe(int index)
        {
            throw new NotImplementedException();
        }
    }
}