using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class UVDemo : MonoBehaviour
{
    private const string SHADER_NAME = "Tutorials/UV Debug Advanced";

    private const string ENABLE_TILING = "_Enable";
    private const string TILING = "_Tiling";
    private const string OFFSET = "_Offset";
    
    private const string ENABLE_TWIRL = "_Enable_1";
    private const string TWIRL_CENTER = "_Center";
    private const string TWIRL_STRENGTH = "_Strength_1";
    private const string TWIRL_OFFSET = "_Offset_1";
    private const string ENABLE_SPHERIZE = "_Enable_2";
    private const string SPHERE_STRENGTH = "_Strength";
    private const string SPHERE_CENTER = "_Center_1";
    private const string SPHERE_OFFSET = "_Offset_2";
    private const string ENABLE_POLAR = "_Enable_3";
    private const string POLAR_RADIAL_LENGTH = "_Radial_Length";
    private const string POLAR_RADIAL_SCALE = "_Radial_Scale";
    private const string ENABLE_NOISE = "_Enable_4";
    private const string NOISE_TILING = "_Noise_Tiling";
    private const string NOISE_SCALE = "_Noise_Scale";
    private const string NOISE_OPACITY = "_Noise_Opacity";
    private const string ENABLE_MOTION = "_Enable_4";
    private const string MOTION_SPEED = "_Motion_Speed";
    private const string MOTION_DIRECTION = "_Motion_Direction";

    [SerializeField] Tiling_and_Offset tiling_and_offset;
    [SerializeField] TwirlUV twirl;
    [SerializeField] SpherizeUV spherize;
    [SerializeField] PolarUV polar;
    [SerializeField] Noise_UV noise;
    [SerializeField] Motion_UV motion;
    
    
    private ShaderModule[] _modules;
    private Shader tutorialShader;
    private Material _tutorialMaterial;
    private Renderer _renderer;
    private void Awake()
    {
        tutorialShader = Shader.Find(SHADER_NAME);
        _tutorialMaterial = new Material(tutorialShader);
        _renderer = GetComponent<Renderer>();
        _renderer.material = _tutorialMaterial;
        _modules = new ShaderModule[]
        {
            tiling_and_offset,
            twirl,
            spherize,
            polar,
            noise,
            motion
        };
    }

    private void Start()
    {
        foreach (var module in _modules)
        {
            module.CopyValuesFromMaterial(_tutorialMaterial);
        }
    }

    private void Update()
    {
        foreach (var module in _modules)
        {
            module.UpdateModule(_tutorialMaterial);
        }
    }


    abstract class ShaderModule
    {
        public bool enabled;
        

        public bool UpdateModule(Material material)
        {
            material.SetFloat(GetEnabledPropertyName(), enabled ? 1 : 0);
            if (enabled)
            {
                UpdateModuleValues(material);
            }
            return enabled;
        }

        public abstract void CopyValuesFromMaterial(Material material);
        
        public abstract void UpdateModuleValues(Material material);
        public abstract  string GetEnabledPropertyName();
    }
    
    

    [Serializable]
    class Tiling_and_Offset : ShaderModule
    {
        
        public Vector2 tiling = Vector2.one / 2f;
        public Vector2 offset = Vector2.zero;

        public override void CopyValuesFromMaterial(Material material)
        {
            tiling = material.GetVector(TILING);
            offset = material.GetVector(OFFSET);
            
        }

        public override void UpdateModuleValues(Material material)
        {
            material.SetVector(TILING, tiling);
            material.SetVector(OFFSET, offset);
        }

        public override string GetEnabledPropertyName() => ENABLE_TILING;
    }
    
    [Serializable]
    class TwirlUV : ShaderModule
    {
        
        public Vector2 center = Vector2.one / 2f;
        public Vector2 offset = Vector2.zero;
        public float strength = 1f;
        public override void CopyValuesFromMaterial(Material material)
        {
            center = material.GetVector(TWIRL_CENTER);
            offset = material.GetVector(TWIRL_OFFSET);
            strength = material.GetFloat(TWIRL_STRENGTH);
            
        }

        public override void UpdateModuleValues(Material material)
        {
            material.SetVector(TWIRL_CENTER, center);
            material.SetVector(TWIRL_OFFSET, offset);
            material.SetFloat(TWIRL_STRENGTH, strength);
        }

        public override string GetEnabledPropertyName()
        {
            return ENABLE_TWIRL;
        }
    }
    
    [Serializable]
    class PolarUV : ShaderModule
    {
        
        public float radialLength;
        public float radialScale;


        public override void CopyValuesFromMaterial(Material material)
        {
            radialLength = material.GetFloat(POLAR_RADIAL_LENGTH);
            radialScale = material.GetFloat(POLAR_RADIAL_SCALE);
            
        }

        public override void UpdateModuleValues(Material material)
        {
            material.SetFloat(POLAR_RADIAL_LENGTH, radialLength);
            material.SetFloat(POLAR_RADIAL_SCALE, radialScale);
        }

        public override string GetEnabledPropertyName()
        {
            return ENABLE_POLAR;
        }
    }
    
    [Serializable]
    class SpherizeUV : ShaderModule
    {
        
        

        public Vector2 center = Vector2.one / 2f;
        public Vector2 offset = Vector2.zero;
        public float strength = 1f;
        public override void CopyValuesFromMaterial(Material material)
        {
            center = material.GetVector(SPHERE_CENTER);
            offset = material.GetVector(SPHERE_OFFSET);
            strength = material.GetFloat(SPHERE_STRENGTH);
            
        }

        public override void UpdateModuleValues(Material material)
        {
            material.SetVector(SPHERE_CENTER, center);
            material.SetVector(SPHERE_OFFSET, offset);
            material.SetFloat(SPHERE_STRENGTH, strength);
        }

        public override string GetEnabledPropertyName() => ENABLE_SPHERIZE;
    }
    [Serializable]
    class Motion_UV : ShaderModule
    {
        
        public float speed = 1f;
        public Vector2 motionDirection = Vector2.right;
        public override void CopyValuesFromMaterial(Material material)
        {
            speed = material.GetFloat(MOTION_SPEED);
            motionDirection = material.GetVector(MOTION_DIRECTION);
        }

        public override void UpdateModuleValues(Material material)
        {
            material.SetFloat(MOTION_SPEED, speed);
            material.SetVector(MOTION_DIRECTION, motionDirection);
        }

        public override string GetEnabledPropertyName()
        {
            return ENABLE_MOTION;
        }
    }
    [Serializable]
    class Noise_UV : ShaderModule
    {
        
        public Vector2 noiseTiling = Vector2.one / 2f;
        public float noiseScale = 1f;
        public float noiseOpacity = 1f;
        public override void CopyValuesFromMaterial(Material material)
        {
            noiseTiling = material.GetVector(NOISE_TILING);
            noiseScale = material.GetFloat(NOISE_SCALE);
            noiseOpacity = material.GetFloat(NOISE_OPACITY);
        }

        public override void UpdateModuleValues(Material material)
        {
            material.SetVector(NOISE_TILING, noiseTiling);
            material.SetFloat(NOISE_SCALE, noiseScale);
            material.SetFloat(NOISE_OPACITY, noiseOpacity);
        }

        public override string GetEnabledPropertyName()
        {
            return ENABLE_NOISE;
        }
    }
}
