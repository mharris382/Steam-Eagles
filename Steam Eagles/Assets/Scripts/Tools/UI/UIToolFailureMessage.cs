using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using Tools.BuildTool;
using UniRx;
using UnityEngine;

namespace Tools.UI
{
    public class UIToolFailureMessage : HUDToolControllerBase
    {
        [Required] public TextMeshProUGUI failureMessageText;

        private CompositeDisposable cd = new CompositeDisposable();
        [SerializeField] private float fadeTime = 0.5f;
        [SerializeField] private float fadeDelay = 1f;

        private Tween _fadeTween;

        void CreateFadeTween()
        {
            if(_fadeTween != null)
                _fadeTween.Kill();
            _fadeTween = DOTween.Sequence()
                .AppendInterval(fadeDelay)
                .Append(failureMessageText.DOFade(0, fadeTime))
                .SetAutoKill(false)
                .Pause();
        }
        
        

        public override void OnFullyInitialized()
        {
            SharedToolData.ErrorMessage.Select(t => !string.IsNullOrEmpty(t)).Subscribe(SetVisible).AddTo(cd);
            SharedToolData.ErrorMessage.Subscribe(SetFailureMessage).AddTo(cd);
            CreateFadeTween();
        }

        public override void HideToolHUD()
        {
            failureMessageText.enabled = false;
        }

        public override void ShowToolHUD(ToolControllerBase controllerBase)
        {
            failureMessageText.enabled = true;
        }

        private void SetFailureMessage(string message)
        {
            failureMessageText.text = message;
            if(_fadeTween == null)CreateFadeTween();
            _fadeTween.Restart();
        }

        private void SetVisible(bool visible)
        {
            //TODO: Add fade in/out
            failureMessageText.enabled = visible;
        }

        private void OnDestroy() => cd.Dispose();
        
        
    }
}