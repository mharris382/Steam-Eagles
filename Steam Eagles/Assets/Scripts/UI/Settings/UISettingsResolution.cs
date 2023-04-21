using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class UISettingsResolution : MonoBehaviour
{
    [Required]
    public TMP_Dropdown resolutionDropdown;
    
    
    private Resolution[] _resolutions;

    private void Awake()
    {
        //get all resolutions available to current screen
        _resolutions = Screen.resolutions;
        
        //determine current resolution
        int currentResolutionIndex = 0;
        for (int i = 0; i < _resolutions.Length; i++)
        {
            if (_resolutions[i].width == Screen.currentResolution.width &&
                _resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        
        //set current resolution
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        
        //add all resolutions to dropdown
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        for (int i = 0; i < _resolutions.Length; i++)
        {
            string option = $"{_resolutions[i].width} x {_resolutions[i].height}";
            options.Add(option);
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    private void SetResolution(int arg0)
    {
        Resolution resolution = _resolutions[arg0];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}
