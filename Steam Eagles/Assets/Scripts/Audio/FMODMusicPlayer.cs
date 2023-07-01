using System;
using FMODUnity;
using Zenject;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class FMODAmbiancePlayer : FMODLoopingEventBase
{
    public FMODAmbiancePlayer([Inject(Id = FMODEventIDs.AMBIANCE)] EventReference eventReference) : base(eventReference)
    {
    }
}

/// <summary>
/// currently just a test class responsible for playing music using FMOD
/// </summary>
public class FMODMusicPlayer : IInitializable, IDisposable
{
    private readonly EventReference _musicEvent;
    private FMOD.Studio.EventInstance _musicInstance;
    private bool _playingMusic;


    public bool IsPlaying
    {
        get => _playingMusic;
        set => SetIsPlaying(value);
    }


    public FMODMusicPlayer([Inject(Id = FMODEventIDs.MUSIC)]EventReference musicEvent)
    {
        _musicEvent = musicEvent;
    }

    /// <summary> called on start </summary>
    public void Initialize()
    {
        _playingMusic = false;
        _musicInstance = RuntimeManager.CreateInstance(_musicEvent);
    }

    /// <summary> called when scene is unloaded (i.e. player returns to main menu) </summary>
    public void Dispose() => _musicInstance.stop(STOP_MODE.IMMEDIATE);
    
    private void SetIsPlaying(bool isPlaying)
    {
        _playingMusic = isPlaying;
        if (isPlaying)
        {
            _musicInstance.start();
        }
        else
        {
            _musicInstance.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }
}