using UnityEngine;

public class UIContinueGameButton : UILoadGameButton
{
    protected override void Awake()
    {
        if (PlayerPrefs.HasKey("Last Save Path"))
        {
            savePath = PlayerPrefs.GetString("Last Save Path");
        }
        base.Awake();
    }
}