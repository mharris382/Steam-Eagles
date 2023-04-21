using UnityEngine;
using UnityEngine.UI;

public class UISettingsFullscreen : Toggle
{
    protected override void Awake()
    {
        base.Awake();
        if(PlayerPrefs.HasKey("Fullscreen"))
            isOn = PlayerPrefs.GetInt("Fullscreen") == 1;
        onValueChanged.AddListener(SetFullscreen);
    }

    private void SetFullscreen(bool on)
    {
        Screen.fullScreen = on;
        PlayerPrefs.SetInt("Fullscreen", on ? 1 : 0);
    }
}