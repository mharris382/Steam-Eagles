using Spine;
using UnityEngine;
using UnityEngine.Events;
using Event = Spine.Event;

public class SpineFootstepEventListener : SpineEventListenerBase
{
    public UnityEvent onFrontFootstep;
    public UnityEvent onBackFootstep;
    public override void OnSpineEvent(TrackEntry trackEntry, Event e)
    {
        Debug.Log("Implement footstep event");
        switch (e.Int)
        {
            case 1:
                onFrontFootstep?.Invoke();
                break;
            case 2:
                onBackFootstep?.Invoke();
                break;
            case 3:
                onFrontFootstep?.Invoke();
                onBackFootstep?.Invoke();
                break;
            default:
                Debug.LogError("Invalid footstep event");
                break;
        }
    }
}