using CoreLib.GameTime;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GasSim.Lighting
{
    public class DayNightSky : DayNightListenerBase, IDayNightListener
    {
        [Required] public Material skyboxMaterial;
        public Renderer[] applyTo;
        static float GetSamplerTime(float time, bool fullDay = false)
        {
            float t = time / (fullDay ? 24 : 12);
            t = Mathf.Sin(t * Mathf.PI - Mathf.PI / 2) * 0.5f + 0.5f;
            return Mathf.Clamp01(t);
        }
        public Vector2Int nightToDayTransitionHours = new Vector2Int(3, 6);
        public Vector2Int dayToNightTransitionHours = new Vector2Int(6, 18);
        private static readonly int DayTime = Shader.PropertyToID("_DayTime");
        public override void OnTimeChanged(GameTime gameTime)
        {
            int hour = gameTime.gameHour;
            if (hour < nightToDayTransitionHours.x || hour > dayToNightTransitionHours.y)
            {
                //nighttime                
                skyboxMaterial.SetFloat(DayTime, 1);
            }
            else if(hour > dayToNightTransitionHours.y && hour < nightToDayTransitionHours.x)
            {
                //daytime
                skyboxMaterial.SetFloat(DayTime, 0);
            }
            else
            {
                int minutes = gameTime.gameMinute + hour * 60;
                float endValue,startValue, transition;
                //transition
                if (hour > nightToDayTransitionHours.x && hour < nightToDayTransitionHours.y)
                {
                    int minutesInTransition = (nightToDayTransitionHours.y - nightToDayTransitionHours.x) * 60;
                     transition = minutes / (float)minutesInTransition;
                    endValue = 1;
                    startValue = 0;
                }
                else
                {
                    int minutesInTransition = (dayToNightTransitionHours.y - dayToNightTransitionHours.x) * 60;
                     transition = minutes / (float)minutesInTransition;
                    endValue = 0;
                    startValue = 1;
                }
                float result = Mathf.Lerp(startValue, endValue, transition);
                skyboxMaterial.SetFloat(DayTime, result);
                return;
            }
            // float samplerTimeHour = GetSamplerTime(gameTime.gameHour);
            // float samplerTimeNextHour = GetSamplerTime(gameTime.gameHour + 1);
            // float samplerTime = Mathf.Lerp(samplerTimeHour, samplerTimeNextHour, gameTime.gameMinute / 60f);
            // skyboxMaterial.SetFloat(DayTime, samplerTime);
        }
    }
}