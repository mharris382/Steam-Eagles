using System;
using CoreLib;
using ObjectLabelMapping;
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

    public string surfaceParameterName = "Surface";
    [SerializeField] private float castDistance = 0.5f;
    [SerializeField] private float castOffset = 0.5f;
    [SerializeField] private LayerMask castLayerMask;
    
    
    
    
    LayerMask _footstepLayerMask;
    private void Awake()
    {
        _footstepLayerMask = castLayerMask;
    }
    

    public override void OnSpineEvent(TrackEntry trackEntry, Event e)
    {
        void CheckPositionForSurface(Vector3 vector3, int value)
        {
            if (TryGetSurfaceParameterFromCast(vector3, out var label2))
                MessageBroker.Default.Publish(new FootstepEventInfo(vector3, value, label2));
            else
                MessageBroker.Default.Publish(new FootstepEventInfo(vector3, value));
        }

        Debug.Log("Implement footstep event");
        var frontFootPosition = frontFoot.position;
        var backFootPosition = backFoot.position;
        switch (e.Int)
        {
            case 0:
            case 1:
                onFrontFootstep?.Invoke();
                CheckPositionForSurface(frontFootPosition, FRONT_FOOT);
                break;
            case 2:
                onBackFootstep?.Invoke();
               CheckPositionForSurface(backFootPosition, BACK_FOOT);
                break;
            case 3:
                onFrontFootstep?.Invoke();
                onBackFootstep?.Invoke();
                CheckPositionForSurface((frontFootPosition + backFootPosition)/2f, BOTH_FEET);
                break;
            default:
                Debug.LogError("Invalid footstep event");
                break;
        }
    }

    bool TryGetSurfaceParameterFromCast(Vector3 origin, out string label)
    {
        var offset = Vector3.up * castOffset;
        label = null;
        var hit = Physics2D.Raycast(origin + offset, Vector2.down, castDistance, _footstepLayerMask);
        Debug.DrawRay(origin +offset, Vector3.down * castDistance, Color.red, 1f);
        if (!hit)
            return false;

        if (hit.collider.TryGetLabel(surfaceParameterName, out label))
            return true;
        
        var sr = hit.collider.GetComponent<SpriteRenderer>();
        if (sr != null && sr.TryGetLabel(surfaceParameterName, out label))
        {
            return true;
        }
        return false;
    }
}