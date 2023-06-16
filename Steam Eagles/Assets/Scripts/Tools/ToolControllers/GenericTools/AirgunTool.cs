using System;
using System.Collections.Generic;
using Buildings;
using CoreLib;
using Items;
using UniRx;
using UnityEngine;
using ToolControllerBase = Tools.BuildTool.ToolControllerBase;

namespace Tools.GenericTools
{
    public class AirgunTool : ToolControllerBase
    {
        public ParticleSystem particleSystem;
        public float emissionRate = 1;
        public int emissionCount = 1;


        private Subject<Unit> _emit = new();


        protected override void OnAwake()
        {
            _emit.Buffer(TimeSpan.FromSeconds(emissionRate)).Where(t => t.Count > 0).Subscribe(_ =>
                particleSystem.Emit(new ParticleSystem.EmitParams()
                {

                }, emissionCount));
        }

        public override ToolStates GetToolState()
        {
            return ToolStates.HAND_BOTH;
        }

        public override BuildingLayers GetTargetLayer()
        {
            return BuildingLayers.NONE;
        }

        public override bool UsesRecipes(out List<Recipe> recipes)
        {
            recipes = new List<Recipe>();
            return false;
        }

        private void Update()
        {
            AimHandler.UpdateAimDirection();
            var direction = base.AimHandler.AimDirection;
            var angle = AimHandler.AimAngle;
            particleSystem.transform.rotation = Quaternion.Euler(0, 0, angle);
            if (IsUseHeld)
            {
                _emit.OnNext(Unit.Default);                
            }
        }
    }
}