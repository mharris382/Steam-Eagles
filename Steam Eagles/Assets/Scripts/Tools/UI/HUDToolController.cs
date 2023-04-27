using System;
using UI.Extensions;
using UniRx;
using UnityEngine.UI;

namespace Tools.UI
{
    public class HUDToolController : HUDToolControllerBase
    {
        public Image image;

        public override void OnFullyInitialized() => SharedToolData.ActiveTool.StartWith(SharedToolData.ActiveToolValue).Subscribe(t => t.ApplyIcon(image)).AddTo(this);
    }
}