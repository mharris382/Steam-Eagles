using Spine;
using UnityEngine;

public abstract class SpineEventListenerBase : MonoBehaviour
{

    public abstract void OnSpineEvent(TrackEntry trackEntry, Spine.Event e);
}