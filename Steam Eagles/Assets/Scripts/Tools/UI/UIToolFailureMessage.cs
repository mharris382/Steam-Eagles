using System;
using Sirenix.OdinInspector;
using TMPro;
using UniRx;
using UnityEngine;

namespace Tools.UI
{
    public class UIToolFailureMessage : HUDToolControllerBase
    {
        [Required] public TextMeshProUGUI failureMessageText;

        private CompositeDisposable cd = new CompositeDisposable();

        public override void OnFullyInitialized()
        {
            SharedToolData.ErrorMessage.Select(t => !string.IsNullOrEmpty(t)).Subscribe(SetVisible).AddTo(cd);
            SharedToolData.ErrorMessage.Subscribe(SetFailureMessage).AddTo(cd);
        }

        private void SetFailureMessage(string message)
        {
            failureMessageText.text = message;
        }

        private void SetVisible(bool visible)
        {
            //TODO: Add fade in/out
            failureMessageText.enabled = visible;
        }

        private void OnDestroy() => cd.Dispose();
    }
}