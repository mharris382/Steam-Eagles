using System;
using System.Linq;
using Characters;
using CoreLib;
using DG.Tweening;
using Items;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Tools.UI
{
    public class PCUI : MonoBehaviour
    {
          
        [Range(0,1)]
        [SerializeField] public int playerIndex;
        [SerializeField] public VisibilityConfig visibilityConfig;
        public IReadOnlyReactiveProperty<bool> isUiVisible;
        private ReadOnlyReactiveProperty<PCInstance> _pc;

        [ShowInInspector, BoxGroup("Debug")]
        public bool HasPC
        {
            get => _pc != null && _pc.Value != null;
            
        }
        [ShowInInspector, BoxGroup("Debug")]
        public GameObject Character
        {
            get => HasPC ? _pc.Value.character : null;
        }

        public Camera Camera => HasPC ? _pc.Value.camera.GetComponent<Camera>() : null;

        public Inventory inventory => HasPC ? _pc.Value.character.GetComponentInChildren<Inventory>() : null;

        public ToolState toolState => HasPC ? _pc.Value.character.GetComponent<ToolState>() : null;

        public PlayerInput playerInput => HasPC ? _pc.Value.input.GetComponent<PlayerInput>() : null;
        
        [Serializable]
        public class VisibilityConfig
        {
            [Required] public CanvasGroup canvasGroup;
            public float fadeTime = 0.5f;
            public Ease fadeEase = Ease.OutCubic;
            private Tween _fadeTween;
            public void SetVisible(bool visible)
            {
                if (_fadeTween != null)
                {
                    _fadeTween.Kill();
                    _fadeTween = null;
                }
                _fadeTween = canvasGroup.DOFade(visible ? 1f : 0f, fadeTime).SetEase(fadeEase);
                _fadeTween.Play();
            }
        }
        
        [Inject] void Install(PCRegistry registry)
        {
            var pc = registry.Values.First(t => t.PlayerNumber == playerIndex);
            if (pc.PC == null) return;
            var added = registry.OnValueAdded.Where(t => t.PlayerNumber == playerIndex).AsUnitObservable();
            var willChange = registry.OnPCWillChange.Where(t => t == playerIndex).AsUnitObservable();
            var remove = registry.OnValueRemoved.Where(t => t.PlayerNumber == playerIndex).AsUnitObservable();
            var pcStream = added.Select(t => registry.GetInstance(playerIndex));
            pcStream.Subscribe(SetPC).AddTo(this);
            _pc = pcStream.ToReadOnlyReactiveProperty();
            var hasPlayer = added.Select(_ => true).Merge(remove.Select(t => false));
            isUiVisible = pc.PCTracker.OnPcBuildingChanged().Select(t => t != null).CombineLatest(hasPlayer, (hasBuilding, hasPl) =>  hasPl && hasBuilding).ToReactiveProperty();
            isUiVisible.Subscribe(SetVisible).AddTo(this);
        }
        
        public virtual void SetPC(PCInstance pc)
        {
            
        }
        
        public virtual void SetVisible(bool visible)
        {
            visibilityConfig.SetVisible(visible);   
        }
    }
}