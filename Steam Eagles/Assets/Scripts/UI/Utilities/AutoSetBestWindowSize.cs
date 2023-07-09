using System;
using UnityEngine;

namespace UI.Utilities
{
    public class AutoSetBestWindowSize : MonoBehaviour
    {
        private void Start()
        {
            var resolutions = Screen.resolutions;
            var highest = resolutions[^1];
            Screen.SetResolution(highest.width, highest.height, Screen.fullScreen);
        }
    }
}