using System;
using System.Collections;
using Characters.Cameras;
using CoreLib;
using DG.Tweening;
using StateMachine;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class CharacterCameraSwitcher : MonoBehaviour
{
    public enum CameraMode
    {
        SPLIT_SCREEN,
        SHARED_SCREEN,
        DYNAMIC_SCREEN,
        MULTI_DISPLAY
    }
    public float splitScreenDistance = 10;
    [SerializeField] private float transitionDuration = 0.5f; 
    [SerializeField] private CameraRects cameraRects;

    [Header("Cameras")] 
    public Camera tpCamera;
    public Camera bdCamera;
    
    [FormerlySerializedAs("p1")]
    [Header("Shared Cameras")]
    [Tooltip("This is the actual camera which is specifically focusing on player 1.")]
    public SharedCamera transporterCamera;
    [FormerlySerializedAs("p2")] [Tooltip("This is the actual camera which is specifically focusing on player 2.")]
    public SharedCamera builderCamera;
    
    

    [Tooltip("This is used to tell any transporter systems which rely on a camera which camera they should be using.")]
    [FormerlySerializedAs("currentCameraPlayer1")] public SharedCamera activeTransporterCamera;
    [FormerlySerializedAs("currentCameraPlayer2")] public SharedCamera activeBuilderCamera;
    
    [FormerlySerializedAs("p1Transform")] [Header("Shared Transforms")]
    public SharedTransform transformerTransform;
    [FormerlySerializedAs("p2Transform")] public SharedTransform builderTransform;

    [Header("Unity Events")]
    public UnityEvent onSwitchingToSingleScreen;
    public UnityEvent onSwitchedToSingleScreen;
    
    public UnityEvent onSwitchingToSplitScreen;
    public UnityEvent onSwitchedToSplitScreen;


    public bool forceSingleCameraMode = false;

    public CameraMode mode = CameraMode.DYNAMIC_SCREEN;
    
    
    bool HasValues() => transporterCamera.HasValue && builderCamera.HasValue && transformerTransform.HasValue && builderTransform.HasValue;


    public SharedCamera SingleScreenCamera => builderCamera;


    private Vector2 P1Position => HasValues() ? transformerTransform.Value.position : Vector2.zero;
    private Vector2 P2Position => HasValues() ? builderTransform.Value.position : Vector2.zero;

    private Camera Cam1 => tpCamera == null ? transporterCamera.Value : tpCamera;
    private Camera Cam2 => bdCamera == null ? builderCamera.Value : bdCamera;

    
    
    [Serializable]
    private class CameraRects
    {
        [Header("Camera 1")]
        public Rect splitScreenRectCamera1 = new Rect(0, 0, 0.5f, 1f);
        [Space(5)]
        public Rect singleScreenRectCamera1 = new Rect(0, 0, 1, 1f);
        [Header("Camera 2")]
        public Rect splitScreenRectCamera2 = new Rect(0.5f, 0, 0.5f, 1f);
        [Space(5)]
        public Rect singleScreenRectCamera2 = new Rect(1, 0, 0.5f, 1f);

        [Header("Gizmos & Debugging")] public bool hideGizmos;
        public bool showSplitScreenLayout;

        public Color camera1Color = Color.red;
        public Color camera2Color = Color.green;
        public void OnDrawGizmos(Transform t)
        {
            if (hideGizmos) return;
            Rect fullBox = new Rect(t.position, t.lossyScale);
            Gizmos.color = Color.black;
            RectDrawGizmos(fullBox);
            var c1ScreenRect = showSplitScreenLayout ? splitScreenRectCamera1 : singleScreenRectCamera1;
            var c2ScreenRect = showSplitScreenLayout ? splitScreenRectCamera2 : singleScreenRectCamera2;
            var c1MinY = Mathf.Lerp(fullBox.min.y, fullBox.max.y, c1ScreenRect.y);
            var c1MinX = Mathf.Lerp(fullBox.min.x, fullBox.max.x, c1ScreenRect.x);
            var c2MinY = Mathf.Lerp(fullBox.min.y, fullBox.max.y, c2ScreenRect.y);
            var c2MinX = Mathf.Lerp(fullBox.min.x, fullBox.max.x, c2ScreenRect.x);
            var c1Pos = new Vector2(c1MinX, c1MinY);
            var c2Pos = new Vector2(c2MinX, c2MinY);
            
            var c1Width = Mathf.Lerp(0, fullBox.width, c1ScreenRect.width);
            var c1Height =  Mathf.Lerp(0, fullBox.height, c1ScreenRect.height);
            var c2Width = Mathf.Lerp(0, fullBox.width, c2ScreenRect.width);
            var c2Height =  Mathf.Lerp(0, fullBox.height, c2ScreenRect.height);
            var c1Rect = new Rect(c1Pos, new Vector2(c1Width, c1Height));
            var c2Rect = new Rect(c2Pos, new Vector2(c2Width, c2Height));
            Gizmos.color = camera1Color;
            RectDrawGizmos(c1Rect);
            Gizmos.color = camera2Color;
            RectDrawGizmos(c2Rect);
        }

        public void RectDrawGizmos(Rect r)
        {
            var positions = new Vector2[5]
            {
                new Vector2(r.min.x, r.min.y),
                new Vector2(r.max.x, r.min.y),
                new Vector2(r.max.x, r.max.y),
                new Vector2(r.min.x, r.max.y),
                new Vector2(r.min.x, r.min.y)
            };
            for (int i = 1; i < positions.Length; i++)
            {
                var p0 = positions[i - 1];
                var p1 = positions[i];
                Gizmos.DrawLine(p0, p1);
            }
        }
    }


    private Rect Cam1Rect
    {
        get => Cam1.rect;
        set => Cam1.rect = value;
    }

    private Rect Cam2Rect
    {
        get => Cam2.rect;
        set => Cam2.rect = value;
    }
    
    private static Vector2 _camera2SingleSize = new Vector2(0, 1);
    private static Vector2 _camera2SplitScreenSize = new Vector2(0, 1);
    private static Vector2 _camera1SingleSize = new Vector2(0, 1);
    private static Vector2 _camera1SplitScreenSize = new Vector2(0, 1);
    private Sequence _switchSequence;

    
    
    private IEnumerator Start()
    {
        while (!HasValues())
        {
            yield return null;
        }

        CompositeDisposable compositeDisposable = new CompositeDisposable();

        //notifies other systems which camera is currently being used by changing the shared reference 
        onSwitchedToSingleScreen.AsObservable().Subscribe(_ => {
            if(activeTransporterCamera!=null)
                activeTransporterCamera.Value = transporterCamera.Value;
            if(activeBuilderCamera!=null)
                activeBuilderCamera.Value = builderCamera.Value;
        }).AddTo(compositeDisposable);
        onSwitchingToSingleScreen.AsObservable().Subscribe(_ => {
            if(activeTransporterCamera!=null)
                activeTransporterCamera.Value = SingleScreenCamera.Value;
            if(activeBuilderCamera!=null)
                activeBuilderCamera.Value = SingleScreenCamera.Value;
        }).AddTo(compositeDisposable);
        
        if (_switchSequence == null)
        {
            this._switchSequence = DOTween.Sequence();
            Cam1.rect = cameraRects.singleScreenRectCamera1;
            Cam2.rect = cameraRects.singleScreenRectCamera2;
            _switchSequence.SetAutoKill(false);
            _switchSequence.Insert(0, Cam1.DORect(cameraRects.splitScreenRectCamera1, transitionDuration));
            _switchSequence.Insert(0, Cam2.DORect(cameraRects.splitScreenRectCamera2, transitionDuration));
            _switchSequence.OnRewind(() => onSwitchingToSingleScreen?.Invoke());
            _switchSequence.OnPlay(() => onSwitchingToSplitScreen?.Invoke());
            _switchSequence.OnComplete(() => onSwitchedToSplitScreen?.Invoke());

            var tweens = this.GetComponentsInChildren<ISplitScreenTween>();
            foreach (var splitScreenTween in tweens)
            {
                float atPosition = 0;
                var tween = splitScreenTween.ToSplitScreenTween(transitionDuration, ref atPosition);
                _switchSequence.Insert(atPosition, tween);
            }
        
            Vector2 diff = P1Position - P2Position;
            bool inSplitScreenMode = diff.sqrMagnitude > (splitScreenDistance * splitScreenDistance) && !forceSingleCameraMode;
        
            if(inSplitScreenMode) onSwitchingToSplitScreen?.Invoke();
            else onSwitchingToSingleScreen?.Invoke();
        
            _switchSequence.Goto((inSplitScreenMode ? 1 : 0)*transitionDuration);
            if (_switchSequence.IsPlaying())
            {
                _switchSequence.Pause();
            }
        }
    }

    private void Update()
    {
        if (!HasValues()) return;
        Vector2 diff = P1Position - P2Position;
        bool inSplitScreenMode = !forceSingleCameraMode && (diff.sqrMagnitude > (splitScreenDistance * splitScreenDistance));

        if (inSplitScreenMode)
        {
            SwitchToSplitScreen();
        }
        else
        {
            SwitchToSingleScreen();
        }
        
        
    }

    void SwitchToSplitScreen(bool instantSwitch = false)
    {
        if (!HasValues()) return;
        //if (inSplitScreen.HasValue && inSplitScreen.Value) return;
        
        _switchSequence.PlayForward();
        if (instantSwitch)
        {
            _switchSequence.GotoWithCallbacks(transitionDuration);
        }
    }

    void SwitchToSingleScreen(bool instantSwitch = false)
    {
        if (!HasValues()) return;
        //if (inSplitScreen.HasValue && !inSplitScreen.Value) return;
        
        _switchSequence.PlayBackwards();
        if (instantSwitch)
        {
            _switchSequence.GotoWithCallbacks(0);
        }
    }
    
    
    
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        cameraRects.OnDrawGizmos(transform);
    }
    #endif
}