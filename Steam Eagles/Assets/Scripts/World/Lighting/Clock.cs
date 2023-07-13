using CoreLib.GameTime;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GasSim.Lighting
{
    public class Clock : DayNightListenerBase
    {
        [Required, ChildGameObjectsOnly] public Transform hourHand;
        [Required, ChildGameObjectsOnly] public Transform minuteHand;


        [ShowInInspector, OnValueChanged(nameof(TestTime)), HideInPlayMode]
        private GameTime testTime;

        void TestTime(GameTime time)
        {
            SetMinuteHand(time.gameMinute);
            SetHourHand(time.gameHour);
        }
        public override void OnTimeChanged(GameTime gameTime)
        {
            SetMinuteHand(gameTime.gameMinute);
            SetHourHand(gameTime.gameHour);
        }

        void SetMinuteHand(int minutes)
        {
            if (minuteHand == null)
            {
                minuteHand = new GameObject("Minute Hand").transform;
                minuteHand.SetParent(transform, false);
            }
            float degreePerMinute = 1 / (float)60;
            float degree = degreePerMinute * minutes;
            minuteHand.localRotation = Quaternion.Euler(0, 0, degree);
        }
        void SetHourHand(int hour)
        {
            if(hourHand == null)
            {
                hourHand = new GameObject("Hour Hand").transform;
                hourHand.SetParent(transform,false);
            }
            float degreePerHour = 1 / (float)12;
            float degree = degreePerHour * hour;
            hourHand.localRotation = Quaternion.Euler(0, 0, degree);
        }
    }
}