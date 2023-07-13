using System;
using CoreLib.GameTime;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GasSim.Lighting
{
    public class DayNightGlobalLight : DayNightListenerBase, IDayNightListener
    {
        [Required]
        public Light2D light;
        [OnValueChanged(nameof(OnGradientChanged), true), GradientUsage(true)]
        public Gradient colorGradient = new Gradient() {
            alphaKeys = new []{new GradientAlphaKey(1, 1), new GradientAlphaKey(0.5f, 0.5f), new GradientAlphaKey(1, 1)},
            colorKeys = new []{new GradientColorKey(Color.white, 0),new GradientColorKey(Color.white, 0.5f), new GradientColorKey(Color.white, 1)}
        };

    
        
        [ShowInInspector, Wrap(0, 1), OnValueChanged(nameof(OnTestValueChanged)), HideInPlayMode]
        private float testValue = 0.5f;

        void OnTestValueChanged(float test)
        {
            if (Application.isPlaying) return;
            if (light == null) return;
            light.color = colorGradient.Evaluate(test);
        }

        void OnGradientChanged(Gradient _)
        {
            if (Application.isPlaying) return;
            if (light == null) return;
            
            light.color = colorGradient.Evaluate(testValue);
        }

        public override void OnTimeChanged(GameTime gameTime)
        {
            if (light == null) return;
            float percent = gameTime.GetPercentageOfDay();
            light.color = colorGradient.Evaluate(percent);
        }
    }
}