using System;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Rendering.Universal;
using UnityEngine.U2D;
#if UNITY_EDITOR
    
using UnityEditor;
using Sirenix.OdinInspector.Editor;
#endif
namespace GasSim.Lighting
{
    public class DayNightController : MonoBehaviour
    {

        [InfoBox("$Label")]
        [Wrap(0, 24), OnValueChanged(nameof(UpdateTime))]
        public float currentTime = 0;

        string Label
        {
            get
            {
                return $"Sample Time: {GetSamplerTime(currentTime)}\n Full Time: {GetSamplerTime(currentTime, true)}";
            }
        }
        
        public DayNightGlobalLight[] globalLights;
        public NighttimeLight[] nightLights;
        
        public Material skyboxMaterial;
        private static readonly int DayTime = Shader.PropertyToID("_DayTime");

       static float GetSamplerTime(float time, bool fullDay = false)
        {
            float t = time / (fullDay ? 24 : 12);
            t = Mathf.Sin(t * Mathf.PI - Mathf.PI / 2) * 0.5f + 0.5f;
            return Mathf.Clamp01(t);
        }
        void UpdateTime(float currentTime)
        {
            float samplerTime = GetSamplerTime(this.currentTime);
            foreach (var dayNightGlobalLight in globalLights)
            {
                dayNightGlobalLight.UpdateLight(samplerTime);
            }

            if (skyboxMaterial != null)
            {
                skyboxMaterial.SetFloat(DayTime, samplerTime);
            }

            foreach (var nighttimeLight in nightLights)
            {
                nighttimeLight.UpdateLight(GetSamplerTime(currentTime, true));
            }
        }
        
        
        [System.Serializable]
        public class DayNightGlobalLight
        {
            [Required]
            public Light2D light;
            [MinMaxRange(0, 2)] public Vector2 intensityRange = new Vector2(0.09f, 0.8f);
            
            public Gradient colorGradient = new Gradient() {
                alphaKeys = new []{new GradientAlphaKey(1, 1), new GradientAlphaKey(0.5f, 0.5f), new GradientAlphaKey(1, 1)},
                colorKeys = new []{new GradientColorKey(Color.white, 0),new GradientColorKey(Color.white, 0.5f), new GradientColorKey(Color.white, 1)}
            };
            
            
            public void UpdateLight(float time)
            {
                if (light == null) return;
                light.intensity = Mathf.Lerp(intensityRange.x, intensityRange.y, time);
                light.color = colorGradient.Evaluate(time);
            }
        }
        
        
        [System.Serializable]
        public class NighttimeLight
        {
            public Light2D[] lights;
            [SerializeField] private bool invert = true;
            [Wrap(0,1)] [SerializeField] private float onTime = .9f;
            [Wrap(0,1)] [SerializeField] private  float offTime = .1f;
           
            public void UpdateLight(float time)
            {
                bool on = invert ? (time > onTime || time < offTime) : (time < onTime || time > offTime);

                foreach (var light in lights)
                {
                    if (light == null) continue;
                    light.enabled = on;
                }
            }
        }

       
    }
    
    #if UNITY_EDITOR
    
    
    #endif
}