using CoreLib;
using Sirenix.OdinInspector;
using Spine;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using Event = Spine.Event;

public class SpineFootstepEventListener : SpineEventListenerBase
{
    public const int FRONT_FOOT = 1;
    public const int BACK_FOOT = 2;
    public const int BOTH_FEET = 3;
    [Required] public Transform frontFoot;
    [Required] public Transform backFoot;
    public UnityEvent onFrontFootstep;
    public UnityEvent onBackFootstep;
    public override void OnSpineEvent(TrackEntry trackEntry, Event e)
    {
        Debug.Log("Implement footstep event");
        var frontFootPosition = frontFoot.position;
        var backFootPosition = backFoot.position;
        switch (e.Int)
        {
            case 1:
                onFrontFootstep?.Invoke();
                MessageBroker.Default.Publish(new FootstepEventInfo(frontFootPosition, FRONT_FOOT));
                break;
            case 2:
                onBackFootstep?.Invoke();
                MessageBroker.Default.Publish(new FootstepEventInfo(backFootPosition, BACK_FOOT));
                break;
            case 3:
                onFrontFootstep?.Invoke();
                onBackFootstep?.Invoke();
                MessageBroker.Default.Publish(new FootstepEventInfo((frontFootPosition + backFootPosition)/2f, BOTH_FEET));
                break;
            default:
                Debug.LogError("Invalid footstep event");
                break;
        }
    }
}