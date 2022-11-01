using System;
using UniRx;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Slider), typeof(TMPro.TextMeshProUGUI))]
    public class UIVolumeSlider : MonoBehaviour
    {
        public Slider volumeSlider;

        [Tooltip("Minimum Volume that the slider allows")] public float minDecibel = -30;
        
        public AudioMixerGroup mixerGroup;

        

        public void Awake()
        {
            string propertyName = $"{GetComponent<TMPro.TextMeshProUGUI>().text = mixerGroup.name} Volume";
            volumeSlider = GetComponent<Slider>();
            if (!mixerGroup.audioMixer.GetFloat(propertyName, out var maxValue))
            {
                Debug.LogError($"Couldn't find property named {propertyName} in audioMixer", mixerGroup.audioMixer);
                Debug.LogError("Volume Slider will not work!", this);
                return;
            }
            volumeSlider.minValue = minDecibel;
            volumeSlider.maxValue = maxValue;
            volumeSlider.normalizedValue = 1;
            volumeSlider.onValueChanged.AsObservable().TakeUntilDestroy(this).Subscribe(t => mixerGroup.audioMixer.SetFloat(propertyName, t));
        }
    }
}