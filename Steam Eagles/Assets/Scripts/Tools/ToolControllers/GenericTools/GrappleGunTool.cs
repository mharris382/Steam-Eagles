using System;
using System.Collections.Generic;
using Buildings;
using CoreLib;
using Items;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using ToolControllerBase = Tools.BuildTool.ToolControllerBase;

namespace Tools.GenericTools
{
    public class GrappleGunTool : ToolControllerBase
    {
        [Required, ChildGameObjectsOnly] public Transform gunPivot;

        private Subject<Vector3> _onFired = new Subject<Vector3>();

        protected override void OnAwake()
        {
            
        }

        private void Update()
        {
            AimHandler.UpdateAimDirection();
            gunPivot.rotation = Quaternion.Euler(0, 0, AimHandler.AimAngle);
            var dir = (Vector2)gunPivot.right;
            var facingRight = Vector2.Dot(dir, Vector2.right) > 0;
            gunPivot.localScale = new Vector3(1, facingRight ? 1 : -1, 1);
            if(IsUseHeld) _onFired.OnNext(gunPivot.right);
        }

        public override bool CanBeUsedOutsideBuilding()
        {
            return true;
        }
        public override ToolStates GetToolState()
        {
            return ToolStates.HAND_RIGHT;
        }
        public override BuildingLayers GetTargetLayer()
        {
            return BuildingLayers.PIPE;
        }
        public override bool UsesRecipes(out List<Recipe> recipes)
        {
            recipes = null;
            return false;
        }
    }
}