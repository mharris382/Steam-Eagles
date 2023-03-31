using UnityEngine;

namespace UI
{
    /// <summary>
    /// used in the UI menu for the continue button
    /// </summary>
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
}