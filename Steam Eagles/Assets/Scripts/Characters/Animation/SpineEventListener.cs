using UnityEngine;
using Spine;
using Spine.Unity;
using UniRx;

public class SpineEventListener : MonoBehaviour
{
    private SkeletonAnimation skeletonAnimation;

    private Subject<(TrackEntry track, Event e)> _onSpineEvent = new();

    public UnityEvents[] events;

    [System.Serializable]
    public class UnityEvents
    {
        public string eventName;
        public UnityEvent onSpineEvent;
        public bool debug;
        internal void OnEvent((TrackEntry track, Event e) data)
        {
            onSpineEvent?.Invoke();
            if(debug){
                Debug.Log($"Received {eventName} spine event");
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

    private void HandleSpineEvent(TrackEntry trackEntry, Event e)
    {
        // Handle the Spine event here
        Debug.Log("Received Spine event: " + e.Data.Name);
        _onSpineEvent.OnNext((trackEntry, e));
    }


    public IObservable<(TrackEntry track, Event e)> OnSpineEvent(string eventName){
        return _onSpineEvent.Where(t => t.e.Data.Name == eventName);
    }
}