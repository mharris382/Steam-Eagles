using System;
using Sirenix.OdinInspector;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace UI
{
    //[RequireComponent(typeof(Slider), typeof(TMPro.TextMeshProUGUI))]
    public class UIVolumeSlider : MonoBehaviour
    {
        public Slider volumeSlider;
        public TextMeshProUGUI sliderLabel;
        public TextMeshProUGUI volumePercentage;

        [SerializeField,TableList]
        private SliderColorGradient[] colorGradients;

        [Tooltip("Minimum Volume that the slider allows")] public float minDecibel = -30;

        public AudioMixerGroup mixerGroup;

        private string propertyName;
        public void Awake()
        {
             propertyName = $"{sliderLabel.text = mixerGroup.name} Volume";
            
            if (!mixerGroup.audioMixer.GetFloat(propertyName, out var maxValue))
            {
                Debug.LogError($"Couldn't find property named {propertyName} in audioMixer", mixerGroup.audioMixer);
                Debug.LogError("Volume Slider will not work!", this);
                return;
            }

            volumeSlider.minValue = minDecibel;
            volumeSlider.maxValue = maxValue;

            if (PlayerPrefs.HasKey(propertyName))
            {
                volumeSlider.normalizedValue = PlayerPrefs.GetFloat(propertyName);                
                OnVolumeChanged(volumeSlider.value);
            }
            else
            {
                volumeSlider.normalizedValue = 1;
                volumePercentage.text = $"{Mathf.RoundToInt(volumeSlider.normalizedValue * 100)}%";
            }
            UpdateColors();
            volumeSlider.onValueChanged.AsObservable().TakeUntilDestroy(this)
                .Subscribe(OnVolumeChanged);
        }
        
        void OnVolumeChanged(float volume)
        {
            mixerGroup.audioMixer.SetFloat(propertyName, volume);
            if (PlayerPrefs.HasKey(propertyName))
            {
                PlayerPrefs.SetFloat(propertyName, volume);
            }
            volumePercentage.text = $"{Mathf.RoundToInt(volumeSlider.normalizedValue * 100)}%";
            UpdateColors();
        }

        private void UpdateColors()
        {
            foreach (var sliderColorGradient in colorGradients)
            {
                sliderColorGradient.UpdateColor(volumeSlider.normalizedValue);
            }
        }


        [System.Serializable]
        public class SliderColorGradient 
        {
            public Image targetImage;
            public Gradient gradient = new Gradient()
            {
                alphaKeys = new GradientAlphaKey[]{new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1)},
                colorKeys = new GradientColorKey[]{new GradientColorKey(Color.white, 0), new GradientColorKey(Color.red, 1)}
            };
            
            public void UpdateColor(float normalizedValue)
            {
                if (targetImage == null) return;
                targetImage.color = gradient.Evaluate(normalizedValue);
            }
        }
    }
}