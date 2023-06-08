using System;
using FMODUnity;
using Zenject;

using STOP_MODE = FMOD.Studio.STOP_MODE;
public abstract class FMODLoopingEventBase : IInitializable, IDisposable
{
    private readonly EventReference _event;
    private FMOD.Studio.EventInstance _eventInstance;
    private bool _isLooping;

    public bool IsInitialized => _eventInstance.isValid();

    public FMOD.Studio.EventInstance EventInstance => _eventInstance;

    public bool IsPlaying
    {
        get => _isLooping;
        set => SetIsPlaying(value);
    }
    
    public FMODLoopingEventBase(EventReference eventReference)
    {
        _event = eventReference;
    }
    public void Initialize()
    {
        _eventInstance = FMODUnity.RuntimeManager.CreateInstance(_event);
        _isLooping = false;
    }

    public void Dispose()
    {
        _eventInstance.release();
    }
    
    private void SetIsPlaying(bool isPlaying)
    {
        _isLooping = isPlaying;
        if (isPlaying)
        {
            EventInstance.start();
        }
        else
        {
            EventInstance.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }
}