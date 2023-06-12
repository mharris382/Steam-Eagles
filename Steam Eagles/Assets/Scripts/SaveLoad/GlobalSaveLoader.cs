using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreLib;
using Cysharp.Threading.Tasks;
using SaveLoad;
using UnityEngine;
using Zenject;

/// <summary>
/// handles logic for saving and loading the game
/// </summary>
public class GlobalSaveLoader : IInitializable
{
    private readonly PersistenceConfig _config;
    private readonly CoroutineCaller _coroutineCaller;
    private readonly GlobalSavePath _savePath;
    private readonly List<ISaveLoaderSystem> _saveLoadSystems;

    
    private List<LoadOrderAttribute> _loadOrderAttributes = new ();
    private Dictionary<LoadOrderAttribute, List<ISaveLoaderSystem>> _saveLoadHandlers = new();

    public bool IsLoading { get; private set; }
    public bool IsSaving { get; private set; }

    public void Initialize()
    {
        Debug.Log("Initializing Global Save Loader: System Count ="  + _saveLoadSystems.Count);
        foreach (var saveLoaderSystem in _saveLoadSystems)
        {
            var loadOrderAttribute = GetLoadOrderAttribute(saveLoaderSystem) ?? LoadOrderAttribute.Default;

            if(!_saveLoadHandlers.TryGetValue(loadOrderAttribute, out var saveLoadHandlerList))
            {
                saveLoadHandlerList = new List<ISaveLoaderSystem>();
                _saveLoadHandlers.Add(loadOrderAttribute, saveLoadHandlerList);
                _loadOrderAttributes.Add(loadOrderAttribute);
            }
            saveLoadHandlerList.Add(saveLoaderSystem);
        }

        _loadOrderAttributes.Sort((a, b) => a.Order.CompareTo(b.Order));
        
        LogLoadGroups();

        void LogLoadGroups()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Load Groups Found {_loadOrderAttributes.Count}");
            foreach (var loadOrderAttribute in _loadOrderAttributes)
            {
                sb.Append($"\tGroup {loadOrderAttribute.Order}:");
                foreach (var saveLoaderSystem in _saveLoadHandlers[loadOrderAttribute])
                {
                    sb.Append($"  {saveLoaderSystem.GetType().Name}");
                    sb.Append(',');
                }

                sb.Remove(sb.Length - 1, 1);
            }

            _config.Log(sb.ToString());
        }

        LoadOrderAttribute GetLoadOrderAttribute(ISaveLoaderSystem saveLoaderSystem)
        {
            LoadOrderAttribute loadOrderAttribute = LoadOrderAttribute.Default;
            var loadOrderAttributes = saveLoaderSystem.GetType().GetCustomAttributes(typeof(LoadOrderAttribute), true);
            if (loadOrderAttributes.Length > 0) loadOrderAttribute = loadOrderAttributes[0] as LoadOrderAttribute;
            loadOrderAttribute ??= LoadOrderAttribute.Default;
            return loadOrderAttribute;
        }
    }

    #region [Constructor]

    public GlobalSaveLoader(
        PersistenceConfig config,
        CoroutineCaller coroutineCaller,
        GlobalSavePath savePath, 
        List<ISaveLoaderSystem> saveLoadSystems)
    {
        _config = config;
        _coroutineCaller = coroutineCaller;
        _savePath = savePath;
        _saveLoadSystems = saveLoadSystems;
    }

    #endregion

    #region [Helpers]

    private bool CheckPathIsValid(ref string savePath, Action<bool> onSaveFinished)
    {
        savePath = savePath.Replace('\\', '/');
        if (!_savePath.TrySetSavePath(ref savePath))
        {
            _config.Log($"Failed to set Path: {savePath.Bolded().InItalics().ColoredRed()}");
            onSaveFinished?.Invoke(false);

            return false;
        }

        return true;
    }
    private bool CheckIfSavingOrLoading(Action<bool> onSaveFinished)
    {
        if (IsSaving || IsLoading)
        {
            Debug.LogError($"Cannot Save while {(IsSaving ? "Saving" : "Loading")}");
            onSaveFinished?.Invoke(false);
            return true;
        }

        return false;
    }

    #endregion

    #region [Public API]

    #region [Save]

    public async UniTask<bool> SaveGameAsync(string savePath)
    {
        if (CheckIfSavingOrLoading(null))
            return false;
        if (!_savePath.TrySetSavePath(ref savePath))
            return false;
        return await SaveGameAsync();
    }
    public async UniTask<bool> SaveGameAsync()
    {
        if (CheckIfSavingOrLoading(null))
            return false;
        List<bool> results = new List<bool>();
        string savePathRoot = _savePath.FullSaveDirectoryPath;
        foreach (var loadGroup in _loadOrderAttributes)
        {
            var loaders = _saveLoadHandlers[loadGroup];
            
            List<UniTask<bool>> groupLoadTasks = new();
            //get all load tasks in this group
            foreach (var saveLoaderSystem in loaders)
            {
                var savePath = savePathRoot;
                
                //check if the system has a subfolder name, ensure folder exists if so
                if (!string.IsNullOrEmpty(saveLoaderSystem.SubFolderName()))
                {
                    //get the subfolder path
                    savePath = Path.Combine(savePathRoot, saveLoaderSystem.SubFolderName());
                    
                    //ensure folder exists
                    if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
                    
                    _config.Log($"Found SubFolderName: {saveLoaderSystem.SubFolderName()}\n{savePath.InItalics()}");
                }
                //add load task to this load group
                groupLoadTasks.Add(saveLoaderSystem.SaveGameAsync(savePath));
            }
            
            //wait for group to finish
            var groupResult = await UniTask.WhenAll(groupLoadTasks);
            results.Add(groupResult.All(t => t));
        }
        return results.All(t => t);
    }

    #endregion
    
    #region [Load]

    public async UniTask<bool> LoadGameAsync(string loadPath)
    {
        if (CheckIfSavingOrLoading(null))
            return false;
        if (!_savePath.TrySetSavePath(ref loadPath))
            return false;
        return await LoadGameAsync();
    }

    public async UniTask<bool> LoadGameAsync()
    {
        List<bool> results = new List<bool>();
        string savePathRoot = _savePath.FullSaveDirectoryPath;
        
        foreach (var loadGroup in _loadOrderAttributes)
        {
            var loaders = _saveLoadHandlers[loadGroup];
            List<UniTask<bool>> loadTasks = new ();
            foreach (var loader in loaders)
            {
                var savePath = savePathRoot;
                //get the subfolder path
                if (!string.IsNullOrEmpty(loader.SubFolderName()))
                    savePath = Path.Combine(savePathRoot, loader.SubFolderName());
                
                //if directory doesn't already exist then return false unless load system is optional in which case return true
                loadTasks.Add(Directory.Exists(savePath)
                    ? loader.LoadGameAsync(savePath)
                    : SkipLoaderOnDirectoryNotFound(loader));
            }
            //wait for all load tasks to finish
            var groupResult = await UniTask.WhenAll(loadTasks);
            LogGroupResults("Load", loadGroup, loaders, groupResult);
            results.Add(groupResult.All(t => t));
        }
        
        return results.All(t => t);

        UniTask<bool> SkipLoaderOnDirectoryNotFound(ISaveLoaderSystem saveLoaderSystem) => UniTask.FromResult(saveLoaderSystem.IsSystemOptional());
    }

    private void LogGroupResults(string operationName, LoadOrderAttribute group, List<ISaveLoaderSystem> groupSystems, bool[] groupResults)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(operationName);
        sb.Append($" Group {group.Order} Results: ");
        if (groupSystems.Count > 1) sb.AppendLine();
        for (int i = 0; i < groupResults.Length; i++)
            sb.Append(GetResultString(i));
        string GetResultString(int i) => $"{groupSystems[i].GetType().Name.InItalics()} : {(groupResults[i] ? "Success" : "Failed")}";
        _config.Log(sb.ToString());
    }

    #endregion
    
    #endregion
    
    #region [Obsolete]

    [Obsolete("Use Async version instead")]
    public void LoadGame(Action<bool> onLoadFinished) => throw new Exception();

    [Obsolete("Use async version")]
    public void SaveGame(Action<bool> onSaveFinished) => throw new Exception();

    [Obsolete("Use async version")]
    public void SaveGame(string savePath, Action<bool> onSaveFinished)
    {
        if (CheckIfSavingOrLoading(onSaveFinished)) return;

        IsSaving = true;
        _config.Log($"Save Game called: {savePath.InItalics()}");
        
        if (!CheckPathIsValid(ref savePath, onSaveFinished)) return;
        throw new Exception();
        //_coroutineCaller.StartCoroutine(PerformSaveOp(savePath, onSaveFinished));
    }
    
    [Obsolete("Use async version")]
    public void LoadGame(string loadPath, Action<bool> onLoadFinished)
    {
        if (CheckIfSavingOrLoading(onLoadFinished)) return;
        
        _config.Log($"Load Game called: {loadPath.InItalics()}");
        
        if (!CheckPathIsValid(ref loadPath, onLoadFinished)) return;

        throw new Exception();
        //_coroutineCaller.StartCoroutine(PerformLoadOp(loadPath, onLoadFinished));
    }
    
    [Obsolete("Use Async version instead")]
    IEnumerator PerformSaveOp(string path, Action<bool> callback)
    {
        IsSaving = true;
        throw new Exception();
        // yield  return PerformOpGrouped(path, callback, s =>
        // {
        //     IsSaving = false;
        //     return s.SaveGameAsync(path);
        // }, "Save");
    }
    
    [Obsolete("Use Async version instead")]
    IEnumerator PerformLoadOp(string path, Action<bool> callback)
    {
        IsLoading = true;
        yield return PerformOpGrouped(path, callback, s =>
        {
            IsLoading = false;
            return s.LoadGameAsync(path);
        }, "Load");
    }
    
    [Obsolete("Use Async version instead")]
    IEnumerator PerformOp(string path, Action<bool> callback, Func<ISaveLoaderSystem, UniTask<bool>> opGetter, string opName)
    {
        yield return UniTask.ToCoroutine(async () =>
        {
            var saveTasks = new List<UniTask<bool>>(_saveLoadSystems.Select(opGetter));
            
            await UniTask.WhenAll(saveTasks);
            
            bool allSucceeded = true;
            
            for (int i = 0; i < saveTasks.Count; i++)
            {
                if (saveTasks[i].Status != UniTaskStatus.Succeeded)
                {
                    allSucceeded = false;
                    _config.Log($"{opName} Failed for System: {_saveLoadSystems[i].GetType().Name.ColoredRed()}");
                }
            }
            callback?.Invoke(allSucceeded);
            if (allSucceeded)
            {
                _config.Log($"{opName} Completed Successfully: {path.Bolded().InItalics().ColoredGreen()}");
                _savePath.ConfirmPathIsValid();
            }
            else
            {
                _config.Log($"{opName} Completed Unsuccessfully: {path.Bolded().InItalics()}".ColoredRed());
                _savePath.RevertToLastValidSavePath();
            }
        });
    }
    
    [Obsolete("Use Async version instead")]
    IEnumerator PerformOpGrouped(string path, Action<bool> callback, Func<ISaveLoaderSystem, UniTask<bool>> opGetter,
        string opName)
    {
        throw new Exception();
        // yield return UniTask.ToCoroutine(async () =>
        // {
        //     bool allGroupsSucceeded = true;
        //     foreach (var group in _loadOrderAttributes)
        //     {
        //         var groupSucceeded = await PerformOp(path, group, opGetter, opName);
        //         if (!groupSucceeded)
        //         {
        //             _config.Log($"{opName} Failed for Group: {group.Order.ToString().ColoredRed()}");
        //             allGroupsSucceeded = false;
        //         }
        //     }
        //     callback?.Invoke(allGroupsSucceeded);
        //     if (allGroupsSucceeded)
        //     {
        //         _config.Log($"{opName} Completed Successfully: {path.Bolded().InItalics().ColoredGreen()}");
        //         _savePath.ConfirmPathIsValid();
        //     }
        //     else
        //     {
        //         _config.Log($"{opName} Completed Unsuccessfully: {path.Bolded().InItalics()}".ColoredRed());
        //         _savePath.RevertToLastValidSavePath();
        //     }
        // });
    }
    
    [Obsolete("Use Async version instead")]
    async UniTask<bool> PerformOp(string path, LoadOrderAttribute loadGroup, Func<ISaveLoaderSystem, UniTask<bool>> opGetter,
        string opName)
    {
        throw new Exception();
        // var saveTasks = new List<UniTask<bool>>(_saveLoadHandlers[loadGroup].Select(opGetter));
        // bool[] results= await UniTask.WhenAll(saveTasks);
        // bool allSucceeded = true;
        // for (int i = 0; i < _saveLoadHandlers[loadGroup].Count; i++)
        // {
        //     if (!results[i])
        //     {
        //         _config.Log($"{opName} Failed for System: {_saveLoadHandlers[loadGroup][i].GetType().Name.ColoredRed()}");
        //         allSucceeded = false;
        //     }
        // }
        // return allSucceeded;
    }

    #endregion
}