using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UniRx;
using UnityEngine;

namespace PhysicsFun
{
    /// <summary>
    /// handles fading FG walls in and out when the player enters the area.
    /// </summary>
    [RequireComponent(typeof(WallFaderBase))]
    
    public class WallFaderController : MonoBehaviour
    {
        private WallFaderBase _wfb;
        public WallFaderBase wfb => _wfb ? _wfb : (_wfb = GetComponent<WallFaderBase>());

        [SerializeField] float fadeTime = 1f;
        [SerializeField] private Ease fadeEase = Ease.InOutSine;
        [SerializeField] float timeToTriggerUnFade = 5;
        
        

        private Tween _fadeTween;
        private float Alpha
        {
            get => wfb.GetWallAlpha();
            set => wfb.SetWallAlpha(value);
        }

        private BoolReactiveProperty _isFaded = new BoolReactiveProperty();
       // public Collider2D triggerCollider;
       // private CharacterTriggerArea _characterTriggerArea;
        
        // private void Awake()
        // {
        //     if (!triggerCollider.gameObject.TryGetComponent(out _characterTriggerArea))
        //     {
        //         Debug.LogWarning("No CharacterTriggerArea found on trigger collider, adding one now.",triggerCollider);
        //         _characterTriggerArea = triggerCollider.gameObject.AddComponent<CharacterTriggerArea>();
        //     }
        //     
        // }
        //
        // private void Start()
        // {
        //     var exitStream = _characterTriggerArea.onAllCharacterLeftArea.AsObservable().Select(_ => false);
        //     var enterStream = _characterTriggerArea.onAnyCharacterInArea.AsObservable().Select(_ => true);
        //     Observable.Merge(enterStream, exitStream)
        //         .Distinct()
        //         .Subscribe(SetFaded)
        //         .AddTo(this);
        // }

        private void Start()
        {
            _isFaded.Subscribe(SetFaded).AddTo(this);
        }

        public void SetFaded(bool isFaded)
        {
            if (!Application.isPlaying)
            {
                Alpha = isFaded ? 0 : 1;
            }
            else
            {
                _isFaded.Value = isFaded;
            }
            
        }

        void ChangeFadeAnimation(bool isFaded)
        {
            float targetAlpha = isFaded ? 0 : 1;
            if (this._fadeTween != null && _fadeTween.IsActive())
            {
                _fadeTween.Kill();
            }
            
            this._fadeTween = DOTween.To(() => Alpha, x => Alpha = x, targetAlpha, fadeTime)
                .SetEase(fadeEase).SetAutoKill(true);
            
            if (!isFaded) _fadeTween.SetDelay(timeToTriggerUnFade);
            
            _fadeTween.Play();
        
        }
    }
    
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(WallFaderController))]
    public class WallFaderControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            void DrawButtons()
            {
                EditorGUILayout.BeginHorizontal();
                if(GUILayout.Button("Fade In"))
                {
                    ((WallFaderController)target).SetFaded(false);
                }
                if(GUILayout.Button("Fade Out"))
                {
                    ((WallFaderController)target).SetFaded(true);
                }
                EditorGUILayout.EndHorizontal();
            }
            DrawButtons();
            base.OnInspectorGUI();
        }
    }
    #endif
}