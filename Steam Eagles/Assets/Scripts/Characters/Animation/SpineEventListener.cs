using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Spine;
using Spine.Unity;
using UniRx;
using UnityEngine.Events;
using Event = Spine.Event;

public class SpineEventListener : MonoBehaviour
{
    private SkeletonAnimation skeletonAnimation;

    private Subject<(TrackEntry track, Spine.Event e)> _onSpineEvent = new();

    public UnityEvents[] events;

    [System.Serializable]
    public class UnityEvents
    {
        [HorizontalGroup(width:0.8f)]
        [SpineEvent]
        public string eventName;
        [HorizontalGroup(width:0.2f),ToggleLeft]
        public bool debug;
        [FoldoutGroup("Events Listeners")]
        public UnityEvent onSpineEvent;
        [FoldoutGroup("Events Listeners")]
        public SpineEventListenerBase[] listeners;
        internal void OnEvent((TrackEntry track, Spine.Event e) data)
        {
            onSpineEvent?.Invoke();
            if(debug){
                Debug.Log($"Received {eventName} spine event");
            }
            foreach (var listener in listeners)
            {
                listener.OnSpineEvent(data.track, data.e);
            }
        }
    }

    private void Start()
    {
        // Get the SkeletonAnimation component
        skeletonAnimation = GetComponent<SkeletonAnimation>();

        // Register event callback
        skeletonAnimation.state.Event += HandleSpineEvent;
        foreach(var e in this.events){
            OnSpineEvent(e.eventName).Subscribe(e.OnEvent).AddTo(this);
        }
    }

    private void HandleSpineEvent(TrackEntry trackEntry, Spine.Event e)
    {
        // Handle the Spine event here
        Debug.Log("Received Spine event: " + e.Data.Name);
        _onSpineEvent.OnNext((trackEntry, e));
    }


    public IObservable<(TrackEntry track, Spine.Event e)> OnSpineEvent(string eventName){
        return _onSpineEvent.Where(t => t.e.Data.Name == eventName);
    }
}