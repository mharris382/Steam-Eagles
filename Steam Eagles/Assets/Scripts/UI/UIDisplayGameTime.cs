using CoreLib.GameTime;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace UI
{
    [System.Serializable]
    public class UIDisplayGameTime
    {
        [Required]
        public TextMeshProUGUI text;
        
        [InfoBox("@GetExampleMilitary")]
        [HideIf(nameof(useAmPm))]
        public string formatSpecifier = "{0}:{1:00}";
        [InfoBox("@GetExampleAmPm")]
        [ShowIf(nameof(useAmPm))]
        public string amPmFormatSpecifier = "{0}:{1:00} {2}";
        public bool useAmPm = false;
        
        [ToggleGroup(nameof(changeColor))]public bool changeColor = true;
        [ToggleGroup(nameof(changeColor))]public Gradient colorGradient = new Gradient()
        {
            alphaKeys = new GradientAlphaKey[2]
            {
                new GradientAlphaKey(1,0),
                new GradientAlphaKey(1,1)
            },
            colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(Color.blue, 0),
                new GradientColorKey(Color.yellow, 0.25f),
                new GradientColorKey(Color.red, 0.5f),
                new GradientColorKey(Color.yellow, 0.75f),
                new GradientColorKey(Color.blue, 1)
            }
        };


        public void Display(GameTime time)
        {
            if (text == null)
            {
                Debug.LogError("Missing text component");
                return;
            }
            if (useAmPm)
            {
                text.text = GetAmPmString(time);
            }
            else
            {
                text.text = GetMilitaryString(time);
            }
        }
        
        string GetAmPmString(GameTime gameTime)
        {
            bool isAm = gameTime.gameHour < 12;
            if (!isAm) gameTime.gameHour -= 12;
            return  string.Format(amPmFormatSpecifier, gameTime.gameHour, gameTime.gameMinute, (isAm ? "AM" : "PM"));
        }
        
        string GetMilitaryString(GameTime gameTime)
        {
            return string.Format(formatSpecifier, gameTime.gameHour, gameTime.gameMinute);
        }
        
        
        [UsedImplicitly]
        string GetExampleAmPm
        {
            get
            {
                var time = new GameTime(16, 30);
                return GetAmPmString(time);
            }
        }
        

        [UsedImplicitly]
        string GetExampleMilitary
        {
            get
            {
                var time = new GameTime(16, 30);
                return GetMilitaryString(time);
            }
        }
    }
}