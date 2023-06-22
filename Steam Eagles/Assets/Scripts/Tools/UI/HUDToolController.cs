using Tools.BuildTool;
using UI.Extensions;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Tools.UI
{
    
    public class HUDToolController : HUDToolControllerBase
    {
        public CanvasGroup _canvasGroup;
        public Image image;

        public override void OnFullyInitialized()
        {
        }

        public override void HideToolHUD()
        {
            _canvasGroup.alpha = 0;
        }

        public override void ShowToolHUD(ToolControllerBase controllerBase)
        {
            _canvasGroup.alpha = 1;
            controllerBase.ApplyIcon(image);
            Debug.Log($"Showing HUD for {controllerBase.name}");
        }

        void SetHUDVisible(bool isVisible)
        {
            _canvasGroup.alpha = isVisible ? 1 : 0;
        }
    }
}