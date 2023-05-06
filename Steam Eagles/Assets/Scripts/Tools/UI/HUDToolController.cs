using System;
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
            SharedToolData.ActiveTool.StartWith(SharedToolData.ActiveToolValue).Subscribe(t => t.ApplyIcon(image)).AddTo(this);
            SharedToolData.ToolsEquippedProperty.Subscribe(SetHUDVisible).AddTo(this);
        }

        void SetHUDVisible(bool isVisible)
        {
            _canvasGroup.alpha = isVisible ? 1 : 0;
        }
    }
}