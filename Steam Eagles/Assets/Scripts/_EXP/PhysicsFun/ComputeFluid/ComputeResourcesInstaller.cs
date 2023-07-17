using System;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace _EXP.PhysicsFun.ComputeFluid
{
    public class ComputeResourcesInstaller : MonoInstaller
    {

        public ComputeShaders computeShaders;

        
        public override void InstallBindings()
        {
            Container.Bind<ComputeShaders>().FromInstance(computeShaders).AsSingle();
        }
    }


    [Serializable]
    public class ComputeShaders
    {
        [Header("Compute Shader Refs")]
        [Space(2)]
        public ComputeShader StokeNavierShader               ; // Contains the code for Advection, Calculating Divergence of a scalar Field and calculating the final divergence free velocity through velocity - Gradient(Pressure)      
        public ComputeShader SolverShader                    ; // This contains the solvers. At the moment there is only Jacobbi inside, though you can extend it as you wish
        public ComputeShader BorderShader                    ; // Dealing with the four corners and arbitary bounderis
        //public ComputeShader StructuredBufferToTextureShader ; // Series of utility kernels to convert structured buffers to textures
         public ComputeShader ioFromTextureShader                 ; // The kernels that add user input (dye or force, through constant stream, images, mouse input etc)
         public ComputeShader ioFromBufferShader;
        //public ComputeShader StructuredBufferUtilityShader;
    }
}