using System;
using UniRx;
using UnityEngine.UI;

namespace Tools.UI
{
    public class HUDToolController : HUDToolControllerBase
    {
        public Image image;

        public override void OnFullyInitialized()
        {
            SharedToolData.ActiveTool.StartWith(SharedToolData.ActiveToolValue).Subscribe(t => {
                    var toolItemSprite = t.ToolIcon?.GetIcon();
                    if (toolItemSprite != null)
                    {
                        image.enabled = true;
                        image.sprite = toolItemSprite;
                    }
                    else
                    {
                        image.enabled = false;
                    }
                }).AddTo(this);
        }
    }
}