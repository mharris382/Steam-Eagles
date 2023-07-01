using FMODUnity;
using UniRx;
using UnityEngine;
using Zenject;
using FMODEvent = FMODUnity.EventReference;
public class FMODSceneHandler : MonoBehaviour
{
    private FMODMusicPlayer _musicPlayer;
    private FMODAmbiancePlayer _ambiancePlayer;

    [Inject] public void InjectMe(FMODSceneLoadCallback loadCallback, FMODMusicPlayer musicPlayer, FMODAmbiancePlayer ambiancePlayer)
    {
        _musicPlayer = musicPlayer;
        _ambiancePlayer = ambiancePlayer;
        if(loadCallback.IsSceneLoaded.Value==false)
            loadCallback.IsSceneLoaded.Where(t =>t).Subscribe(_ => OnSceneLoaded()).AddTo(this);
        else
            OnSceneLoaded();
    }
    
    public void OnSceneLoaded()
    {
        _musicPlayer.IsPlaying = true;
        _ambiancePlayer.IsPlaying = true;
    }
}