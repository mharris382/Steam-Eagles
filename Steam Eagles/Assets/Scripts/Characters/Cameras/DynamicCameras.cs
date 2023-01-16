using System;
using System.Collections;
using System.Collections.Generic;
using CoreLib;
using DG.Tweening;
using Sirenix.OdinInspector;
using StateMachine;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Characters.Cameras
{
    public class DynamicCameras : MonoBehaviour
    {
        
        public float transitionDuration =0.5f;
        [SerializeField]private float splitScreenDistance = 10;
        
        public SharedCamera p1Camera;
        public SharedCamera p2Camera;
        public SharedTransform p1Transform;
        public SharedTransform p2Transform;
        public BoolReactiveProperty forceSharedCamera = new BoolReactiveProperty(false);

        private Camera P1Cam => p1Camera.Value;
        private Camera P2Cam => p2Camera.Value;
        private Transform P1Transform => p1Transform.Value;
        private Transform P2Transform => p2Transform.Value;
        private Vector2 P1Position => P1Transform.position;
        private Vector2 P2Position => P2Transform.position;
        
        [FoldoutGroup("Events")]public UnityEvent onSwitchedToSharedCamera;
        [FoldoutGroup("Events")]public UnityEvent onSwitchingToSharedCamera;
        [FoldoutGroup("Events")]public UnityEvent onSwitchedToSplitScreenCamera;
        [FoldoutGroup("Events")]public UnityEvent onSwitchingToSplitScreenCamera;
        bool HasResources()
        {
            return p1Camera.HasValue &&
                   p2Camera.HasValue &&
                   p1Transform.HasValue &&
                   p2Transform.HasValue;
        }

        private Rect p1SplitscreenRect = new Rect(0, 0, .5f, 1);
        private Rect p2SplitscreenRect = new Rect(0.5f, 0, .5f, 1);
        private Rect p1FullscreenRect = new Rect(0, 0, 1, 1);
        private Rect p2FullscreenRect = new Rect(1, 0, 1, 1);
        private Sequence _switchSequence;


        ReactiveProperty<bool> _isSplitScreen = new ReactiveProperty<bool>();

        private IEnumerator Start()
        {
            while (!HasResources())
            {
                yield return null;
            }
            CompositeDisposable compositeDisposable = new CompositeDisposable();
            _isSplitScreen = new ReactiveProperty<bool>(false);
            _isSplitScreen.Subscribe(t =>
            {
                if (!t)
                {
                    onSwitchingToSharedCamera?.Invoke();
                }
                else
                {
                    onSwitchingToSplitScreenCamera?.Invoke();
                }
            }).AddTo(this);
            InitSequence();
            compositeDisposable.AddTo(this);
            yield return null;
            if (CheckForSplitScreen())
            {
                SwitchToSplitScreen(true);
            }
            else
            {
                SwitchToSharedCamera(true);
            }
        }

        void InitSequence()
        {
            _switchSequence = DOTween.Sequence();
            P1Cam.rect = p1FullscreenRect;
            P2Cam.rect = p2FullscreenRect;
            _switchSequence.SetAutoKill(false);
            _switchSequence.Insert(0, P1Cam.DORect(p1SplitscreenRect, this.transitionDuration));
            _switchSequence.Insert(0, P2Cam.DORect(p2SplitscreenRect, this.transitionDuration));
            _switchSequence.OnRewind(() => onSwitchedToSharedCamera?.Invoke());
            _switchSequence.OnComplete(() => onSwitchedToSplitScreenCamera?.Invoke());
            
            
        }

        private void Update()
        {
            if (!HasResources()) return;
            bool inSplitscreen = CheckForSplitScreen();
            if (inSplitscreen)
            {
                _isSplitScreen.Value = true;
                SwitchToSplitScreen();
            }
            else
            {
                _isSplitScreen.Value = false;
                SwitchToSharedCamera();
            }
        }
        
        void SwitchToSplitScreen(bool instantSwitch=false)
        {
            if (!HasResources()) return;
            _switchSequence.PlayForward();
            
            if (instantSwitch)
            {
                _switchSequence.GotoWithCallbacks(transitionDuration);
            }
        }
        void SwitchToSharedCamera(bool instantSwitch=false)
        {
            if (!HasResources()) return;
            _switchSequence.PlayBackwards();
            if (instantSwitch)
            {
                _switchSequence.GotoWithCallbacks(0);
            }
        }

        bool CheckForSplitScreen()
        {
            Vector2 diff = P1Position - P2Position;
            return diff.sqrMagnitude > (this.splitScreenDistance * splitScreenDistance) && !forceSharedCamera.Value;
        }
    }
}