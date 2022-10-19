using System;
using System.Collections;
using CoreLib;
using DG.Tweening;
using StateMachine;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class CharacterCameraSwitcher : MonoBehaviour
{

    public float splitScreenDistance = 10;
    [SerializeField] private float transitionDuration = 0.5f; 
    [SerializeField] private CameraRects cameraRects;
    
    [Header("Shared Cameras")]
    [Tooltip("This is the actual camera which is specifically focusing on player 1.")]
    public SharedCamera p1;
    [Tooltip("This is the actual camera which is specifically focusing on player 2.")]
    public SharedCamera p2;
    
    

    
    public SharedCamera currentCameraPlayer1;
    public SharedCamera currentCameraPlayer2;
    
    [Header("Shared Transforms")]
    public SharedTransform p1Transform;
    public SharedTransform p2Transform;

    [Header("Unity Events")]
    public UnityEvent onSwitchingToSingleScreen;
    public UnityEvent onSwitchedToSingleScreen;
    
    public UnityEvent onSwitchingToSplitScreen;
    public UnityEvent onSwitchedToSplitScreen;


    public bool forceSingleCameraMode = false;

    bool HasValues() => p1.HasValue && p2.HasValue && p1Transform.HasValue && p2Transform.HasValue;


    public SharedCamera SingleScreenCamera => p2;


    private Vector2 P1Position => HasValues() ? p1Transform.Value.position : Vector2.zero;
    private Vector2 P2Position => HasValues() ? p2Transform.Value.position : Vector2.zero;

    private Camera Cam1 => p1.Value;
    private Camera Cam2 => p2.Value;

    
    
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

    private bool? inSplitScreen;
    private IEnumerator Start()
    {
        inSplitScreen = null;
        while (!HasValues())
        {
            yield return null;
        }
        onSwitchingToSplitScreen.AddListener(() =>
        {
            if(currentCameraPlayer1!=null)
                currentCameraPlayer1.Value = p1.Value;
            if(currentCameraPlayer2!=null)
                currentCameraPlayer2.Value = p2.Value;
        });
        
        onSwitchingToSingleScreen.AddListener(() =>
        {
            if(currentCameraPlayer1!=null)
                currentCameraPlayer1.Value = SingleScreenCamera.Value;
            if(currentCameraPlayer2!=null)
                currentCameraPlayer2.Value = SingleScreenCamera.Value;
        });
        
        this._switchSequence = DOTween.Sequence();
        Cam1.rect = cameraRects.singleScreenRectCamera1;
        Cam2.rect = cameraRects.singleScreenRectCamera2;
        _switchSequence.SetAutoKill(false);
        
        _switchSequence.Insert(0, Cam1.DORect(cameraRects.splitScreenRectCamera1, transitionDuration));
        _switchSequence.Insert(0, Cam2.DORect(cameraRects.splitScreenRectCamera2, transitionDuration));
        
        _switchSequence.OnRewind(() => onSwitchingToSingleScreen?.Invoke());
        _switchSequence.OnPlay(() => onSwitchingToSplitScreen?.Invoke());
        
        _switchSequence.OnComplete(() => onSwitchedToSplitScreen?.Invoke());
        
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

    private void Update()
    {
        if (!HasValues()) return;
        Vector2 diff = P1Position - P2Position;
        bool inSplitScreenMode = diff.sqrMagnitude > (splitScreenDistance * splitScreenDistance);

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
        if (inSplitScreen.HasValue && inSplitScreen.Value) return;
        inSplitScreen = true;
        _switchSequence.PlayForward();
        if (instantSwitch)
        {
            _switchSequence.GotoWithCallbacks(transitionDuration);
        }
    }

    void SwitchToSingleScreen(bool instantSwitch = false)
    {
        if (!HasValues()) return;
        if (inSplitScreen.HasValue && !inSplitScreen.Value) return;
        inSplitScreen = false;
        _switchSequence.PlayBackwards();
        if (instantSwitch)
        {
            _switchSequence.GotoWithCallbacks(0);
        }
    }
}