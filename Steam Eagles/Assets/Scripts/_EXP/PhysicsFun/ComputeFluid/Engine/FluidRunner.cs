using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace _EXP.PhysicsFun.ComputeFluid.Engine
{
    public class FluidRunner : MonoBehaviour
    {
        public FluidSimulater simulater;
        private GasTexture _gasTexture;

        Coroutine _runCoroutine;
        private FluidGPUResources _gpuResources;

        [Inject]
        void Install(GasTexture gasTexture, FluidSimulater simulater, FluidGPUResources gpuResources)
        {
            _gasTexture = gasTexture;
            _gpuResources = gpuResources;
            this.simulater = simulater;
            simulater.canvas_dimension = simulater.simulation_dimension = gasTexture.ImageSize;
            return;
            simulater.Initialize(gasTexture.RenderTexture);
            InitComputePipeline();
        }


        [Button(ButtonSizes.Gigantic)]
        void AddDyeFromTexture()
        {
        return;
            if (_gasTexture == null) return;
            var newTexture = new Texture2D(_gasTexture.ImageSize.x, _gasTexture.ImageSize.y)
            {
                minimumMipmapLevel = 1,
                requestedMipmapLevel = 1,
                ignoreMipmapLimit = true
            };
            Graphics.CopyTexture(_gasTexture.RenderTexture, newTexture);
            simulater.AddDyeFromTexture (_gpuResources.dye_buffer, newTexture, true);
        }

    void InitComputePipeline()
        {
            return;
            simulater.Initialize(_gasTexture.Dye);
            _gpuResources = new FluidGPUResources(simulater);
            _gpuResources.Create();
            simulater.AddUserForce(_gpuResources.velocity_buffer);
            simulater.HandleCornerBoundaries(_gpuResources.velocity_buffer, FieldType.Velocity);
            simulater.Diffuse(_gpuResources.velocity_buffer);
            simulater.HandleCornerBoundaries(_gpuResources.velocity_buffer, FieldType.Velocity);
            simulater.Project(_gpuResources.velocity_buffer, _gpuResources.divergence_buffer, _gpuResources.pressure_buffer);
            simulater.Advect(_gpuResources.velocity_buffer, _gpuResources.velocity_buffer, 0.9999f);
            simulater.HandleCornerBoundaries(_gpuResources.velocity_buffer, FieldType.Velocity);
            simulater.Project(_gpuResources.velocity_buffer, _gpuResources.divergence_buffer, _gpuResources.pressure_buffer);
            simulater.HandleCornerBoundaries(_gpuResources.velocity_buffer, FieldType.Velocity);
            
            // simulater.AddDye(_gpuResources.dye_buffer);
            // simulater.Advect(_gpuResources.dye_buffer, _gpuResources.dye_buffer, 0.9999f);
            
            simulater.Visualiuse(_gpuResources.dye_buffer);
            simulater.BindCommandBuffer();
        }

        private void OnEnable()
        {
            return;
            if (_gasTexture != null) StartRunning();
        }

        private void OnDisable()
        {
            return;
            StopRunning();
        }

        void StartRunning()
        {
            return;
            if(_runCoroutine != null) return;
            InitComputePipeline();
            _runCoroutine = StartCoroutine(Run());
        }

        public bool getData;
        [FoldoutGroup("Debug"), ShowInInspector, ListDrawerSettings(IsReadOnly = true, NumberOfItemsPerPage =100)] Vector2[] velocityData;
        IEnumerator Run()
        {
            yield break;
            while (true)
            {
                yield return null;
                simulater.Tick(Time.deltaTime);
                if (getData)
                {
                    velocityData = new Vector2[simulater.canvas_dimension.x * simulater.canvas_dimension.y];
                    _gpuResources.velocity_buffer.GetData(velocityData);
                    var list = new List<Vector2>(velocityData);
                    list.Sort((t1, t2) => t1.x == t2.x ? (int)Mathf.Sign(t1.y - t2.y) :(int) Mathf.Sign(t1.x - t2.x));
                    velocityData = list.ToArray();
                    getData = false;
                }
            }
            // ReSharper disable once IteratorNeverReturns
        }

        void StopRunning()
        {
            return;
            if (_runCoroutine != null)
            {
                StopCoroutine(_runCoroutine);
                _runCoroutine = null;
                simulater.Release();
                _gpuResources.Release();
            }
        }
    }
}