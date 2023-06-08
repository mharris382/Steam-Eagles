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
        
        instance.start();
        instance.release();
    }

    public void PlayEventAtPosition(Vector3 position)
    {
        FMODUnity.RuntimeManager.PlayOneShot(_eventReference, position);
    }

    public void PlayEventAtPosition(Vector2 position) => PlayEventAtPosition((Vector3)position);
}