using System.Diagnostics;
using FMODUnity;
using UnityEngine;

public abstract class FMODOneShotEventBase 
{
    private readonly EventReference _eventReference;

    public FMODOneShotEventBase(EventReference eventReference)
    {
        _eventReference = eventReference;
    }


    public void PlayEvent()
    {
        FMOD.Studio.EventInstance instance = RuntimeManager.CreateInstance(_eventReference);
        OnPrePlayEvent(instance);
        instance.start();
        OnPostPlayEvent(instance);
        instance.release();
    }

    protected virtual void OnPrePlayEvent(FMOD.Studio.EventInstance eventInstance)
    {
        
    }
    protected virtual void OnPostPlayEvent(FMOD.Studio.EventInstance eventInstance)
    {
        
    }

    public void PlayEventAtPosition(Vector3 position)
    {
        PlayEvent();
       // FMODUnity.RuntimeManager.PlayOneShot(_eventReference, position);
    }

    public void PlayEventAtPosition(Vector2 position) => PlayEventAtPosition((Vector3)position);
}