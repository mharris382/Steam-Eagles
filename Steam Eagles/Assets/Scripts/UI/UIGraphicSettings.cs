using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGraphicSettings : MonoBehaviour
{
    const string RESOLUTION_KEY = "resolution";
    const string FULLSCREEN_KEY = "fullscreen";
    const string QUALITY_KEY = "quality";

    [SerializeField] Toggle fullscreenToggle;
    [SerializeField] TMPro.TMP_Dropdown qualityDropdown;
    [SerializeField] TMPro.TMP_Dropdown resolutionDropdown;

    private void Awake()
    {
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggle);
        qualityDropdown.onValueChanged.AddListener(OnQualityDropdown);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionDropdown);
        SetupQualityDropdown();
        SetupResolutionDropdown();
        #if !WEBGL
        LoadSettings();
        #endif
    }

    private void OnDestroy()
    {
        SaveSettings();
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt(RESOLUTION_KEY, resolutionDropdown.value);
        PlayerPrefs.SetInt(FULLSCREEN_KEY, fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt(QUALITY_KEY, qualityDropdown.value);
    }
    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey(FULLSCREEN_KEY))
        {
            fullscreenToggle.isOn = PlayerPrefs.GetInt(FULLSCREEN_KEY) == 1;
            OnFullscreenToggle(fullscreenToggle.isOn);
        }
        if (PlayerPrefs.HasKey(QUALITY_KEY))
        {
            int quality = PlayerPrefs.GetInt(QUALITY_KEY);
            if(quality < qualityDropdown.options.Count)
            {
                qualityDropdown.value = quality;
                OnQualityDropdown(quality);
            }
        }
        if (PlayerPrefs.HasKey(RESOLUTION_KEY))
        {
            int resolution = PlayerPrefs.GetInt(RESOLUTION_KEY);
            if(resolution < resolutionDropdown.options.Count)
            {
                resolutionDropdown.value = resolution;
                OnResolutionDropdown(resolution);
            }
        }
    }

    void SetupQualityDropdown()
    {
        var options = new List<TMPro.TMP_Dropdown.OptionData>();
        
        for (int i = 0; i < QualitySettings.names.Length; i++)
            options.Add(new TMPro.TMP_Dropdown.OptionData(QualitySettings.names[i]));
        
        qualityDropdown.options = options;
    }

    void SetupResolutionDropdown()
    {
        var options = new List<TMPro.TMP_Dropdown.OptionData>();
        var resolutions = Screen.resolutions;
        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add(new TMPro.TMP_Dropdown.OptionData(resolutions[i].ToString()));
        }
        resolutionDropdown.options = options;
    }
    void OnFullscreenToggle(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
    
    void OnQualityDropdown(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }
    
    void OnResolutionDropdown(int resolutionIndex)
    {
        Resolution resolution = Screen.resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}
