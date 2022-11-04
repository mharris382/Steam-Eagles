using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace UI
{
    public class SettingsMenu : MonoBehaviour
    {
        const string PREF_RESOLUTION = "resolution";
        const string PREF_FULLSCREEN = "fullscreen";
        [SerializeField] TMPro.TMP_Dropdown resolutionDropdown;
        [SerializeField] Toggle fullscreenToggle;
        private void Awake()
        {
            var supportedResolutions = Screen.resolutions;
            resolutionDropdown.options= new List<TMP_Dropdown.OptionData>(supportedResolutions.Select(t=> new TMP_Dropdown.OptionData(t.ToString())));
            resolutionDropdown.value = Array.IndexOf(supportedResolutions, Screen.currentResolution);
            
            resolutionDropdown.onValueChanged.AsObservable().Select(t=> supportedResolutions[t]).TakeUntilDestroy(this).Subscribe(OnResolutionChanged);
            fullscreenToggle.onValueChanged.AsObservable().TakeUntilDestroy(this).Subscribe(OnFullscreenChanged);

            if (PlayerPrefs.HasKey(PREF_FULLSCREEN))
            {
                fullscreenToggle.isOn = PlayerPrefs.GetInt(PREF_FULLSCREEN) > 0;
            }
            if (PlayerPrefs.HasKey(PREF_RESOLUTION))
            {
                var savedResolution = PlayerPrefs.GetInt(PREF_RESOLUTION);
                if(savedResolution < supportedResolutions.Length)
                    resolutionDropdown.value = savedResolution;
            }
        }

        void OnResolutionChanged(Resolution resolution)
        {
            Screen.SetResolution(resolution.width, resolution.height, fullscreenToggle.isOn);
            PlayerPrefs.SetInt(PREF_RESOLUTION, resolutionDropdown.value);
        }
        
        void OnFullscreenChanged(bool isFullscreen)
        {
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, isFullscreen);
            PlayerPrefs.SetInt(PREF_FULLSCREEN, isFullscreen ? 1 : 0);     
        }
        
    }
}