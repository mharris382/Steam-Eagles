using System.IO;
using System.Linq;
using CoreLib;
using SaveLoad;
using SaveLoad.CoreSave;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Zenject;

public class PersistenceInstaller : MonoInstaller
{
    [HideLabel]
    public PersistenceConfig config;
    public override void InstallBindings()
    {
        Container.Bind<PersistenceConfig>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<GlobalSaveLoader>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<GlobalSavePath>().AsSingle().NonLazy();
        Container.Bind<PersistenceState>().AsSingle().NonLazy();
       //Container.BindInterfacesTo<AsyncCoreSaveDataLoader>().AsSingle().NonLazy();
        Container.Bind<PathValidator>().AsSingle().NonLazy();
         var saveLoadSystems = ReflectionUtils.GetConcreteTypes<ISaveLoaderSystem>();
         foreach (var saveLoadSystem in saveLoadSystems)
         {
             Container.Bind<ISaveLoaderSystem>().To(saveLoadSystem).FromNew().AsSingle();
             config.Log($"Bound {saveLoadSystem.Name}");
         }
    }
}


[InlineProperty]
[System.Serializable]
public class PersistenceConfig : ConfigBase
{
    [FolderPath(AbsolutePath = true)]
    public string testingSavePath;

    bool ValidatePath(string path, ref string msg)
    {
        if (string.IsNullOrEmpty(path))
        {
            msg = "Must provide valid testing path";
            return false;
        }

        msg = "Directory does not exit";
        return Directory.Exists(path);
    }
}


/*public class SaveLoadRequestLogger : IInitializable, IDisposable
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
}*/