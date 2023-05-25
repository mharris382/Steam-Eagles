using System;
using CoreLib;
using UniRx;
using UnityEngine;
using Zenject;

public class PersistenceInstaller : MonoInstaller
{
    public bool logAllRequests = true;
    public override void InstallBindings()
    {
        Container.BindInterfacesTo<SaveLoadRequestProcessor>().AsSingle().NonLazy();
        if (logAllRequests)
        {
            Container.BindInterfacesTo<SaveLoadRequestLogger>().AsSingle().NonLazy();
        }
    }
}

public class SaveLoadRequestLogger : IInitializable, IDisposable
{
    private IDisposable _disposable;
    public void Initialize()
    {
        var cd = new CompositeDisposable();
        Debug.Log("SaveLoadRequestLogger Initialized");
        MessageBroker.Default.Receive<SaveGameRequestedInfo>().Select(t => t.savePath).Subscribe(LogSaveGameRequest).AddTo(cd);
        MessageBroker.Default.Receive<LoadGameRequestedInfo>().Select(t => t.loadPath).Subscribe(LogLoadGameRequest).AddTo(cd);

        _disposable = cd;
    }

    public void Dispose()
    {
        Debug.Log("SaveLoadRequestLogger Disposed");
        _disposable?.Dispose();
        _disposable = null;
    }

    private void LogSaveGameRequest(string savePath) => Debug.Log($"Requested Save: {savePath.Bolded()}".InItalics());

    private void LogLoadGameRequest(string savePath) => Debug.Log($"Requested Load: {savePath.Bolded()}".InItalics());
}



public class SaveLoadRequestProcessor : IInitializable, IDisposable
{
    
    private IDisposable _disposable;
    public void Initialize()
    {
        var cd = new CompositeDisposable();
        Debug.Log("SaveLoadRequestProcessor Initialized");
        MessageBroker.Default.Receive<SaveGameRequestedInfo>().Select(t => t.savePath).Subscribe(ProcessSaveGameRequest).AddTo(cd);
        MessageBroker.Default.Receive<LoadGameRequestedInfo>().Select(t => t.loadPath).Subscribe(ProcessLoadGameRequest).AddTo(cd);

        _disposable = cd;
    }

    public void Dispose()
    {
        Debug.Log("SaveLoadRequestProcessor Disposed");
        _disposable?.Dispose();
        _disposable = null;
    }

    private void ProcessSaveGameRequest(string savePath)
    {
        
    }
    private void ProcessLoadGameRequest(string savePath)
    {
        
    }
}