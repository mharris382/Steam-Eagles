using CoreLib;
using UnityEngine;

namespace UI
{
    public class UIWindowCanvasManager : Singleton<UIWindowCanvasManager>
    {
        public override bool DestroyOnLoad => false;
        [SerializeField] private RectTransform fullScreenParent;
        [SerializeField] private RectTransform leftScreenParent;
        [SerializeField] private RectTransform rightScreenParent;
        
        
        public RectTransform GetScreenParent(ScreenSide screenSide)
        {
            switch (screenSide)
            {
                case ScreenSide.FULL:
                    return fullScreenParent;
                case ScreenSide.LEFT:
                    return leftScreenParent;
                case ScreenSide.RIGHT:
                    return rightScreenParent;
                default:
                    return null;
            }
        }
    }
}