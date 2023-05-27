using System;
using System.Collections;
using System.Collections.Generic;
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
            LoadOrderAttribute loadOrderAttribute = LoadOrderAttribute.Default;
            var loadOrderAttributes = saveLoaderSystem.GetType().GetCustomAttributes(typeof(LoadOrderAttribute), true);
            if (loadOrderAttributes.Length > 0)
                loadOrderAttribute = loadOrderAttributes[0] as LoadOrderAttribute;
            if(!_saveLoadHandlers.TryGetValue(loadOrderAttribute ?? LoadOrderAttribute.Default, out var saveLoadHandlerList))
            {
                saveLoadHandlerList = new List<ISaveLoaderSystem>();
                _saveLoadHandlers.Add(loadOrderAttribute, saveLoadHandlerList);
                _loadOrderAttributes.Add(loadOrderAttribute);
            }
            saveLoadHandlerList.Add(saveLoaderSystem);
        }

        _loadOrderAttributes.Sort((a, b) => a.Order.CompareTo(b.Order));
        
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

    public GlobalSaveLoader(PersistenceConfig config,
        CoroutineCaller coroutineCaller,
        GlobalSavePath savePath, List<ISaveLoaderSystem> saveLoadSystems)
    {
        _config = config;
        _coroutineCaller = coroutineCaller;
        _savePath = savePath;
        _saveLoadSystems = saveLoadSystems;
        
     


    }

    public void SaveGame(Action<bool> onSaveFinished) => SaveGame(_savePath.FullSaveDirectoryPath, onSaveFinished);

    public void SaveGame(string savePath, Action<bool> onSaveFinished)
    {
        if (CheckIfSavingOrLoading(onSaveFinished)) return;

        IsSaving = true;
        _config.Log($"Save Game called: {savePath.InItalics()}");
        
        if (!CheckPathIsValid(ref savePath, onSaveFinished)) return;
        _coroutineCaller.StartCoroutine(PerformSaveOp(savePath, onSaveFinished));
    }

    private bool CheckPathIsValid(ref string savePath, Action<bool> onSaveFinished)
    {
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
            Debug.LogError("Cannot Save while Saving or Loading");
            onSaveFinished?.Invoke(false);
            return true;
        }

        return false;
    }


    public void LoadGame(Action<bool> onLoadFinished) => LoadGame(_savePath.FullSaveDirectoryPath, onLoadFinished);

    public void LoadGame(string loadPath, Action<bool> onLoadFinished)
    {
        if (CheckIfSavingOrLoading(onLoadFinished)) return;
        
        _config.Log($"Load Game called: {loadPath.InItalics()}");
        
        if (!CheckPathIsValid(ref loadPath, onLoadFinished)) return;

        _coroutineCaller.StartCoroutine(PerformLoadOp(loadPath, onLoadFinished));
    }

    IEnumerator PerformSaveOp(string path, Action<bool> callback) => PerformOpGrouped(path, callback, s => s.SaveGameAsync(path), "Save");

    IEnumerator PerformLoadOp(string path, Action<bool> callback) => PerformOpGrouped(path, callback, s => s.LoadGameAsync(path), "Load");

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

    IEnumerator PerformOpGrouped(string path, Action<bool> callback, Func<ISaveLoaderSystem, UniTask<bool>> opGetter,
        string opName)
    {
        yield return UniTask.ToCoroutine(async () =>
        {
            bool allGroupsSucceeded = true;
            foreach (var group in _loadOrderAttributes)
            {
                var groupSucceeded = await PerformOp(path, group, opGetter, opName);
                if (!groupSucceeded)
                {
                    _config.Log($"{opName} Failed for Group: {group.Order.ToString().ColoredRed()}");
                    allGroupsSucceeded = false;
                }
            }
            callback?.Invoke(allGroupsSucceeded);
            if (allGroupsSucceeded)
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

    async UniTask<bool> PerformOp(string path, LoadOrderAttribute loadGroup, Func<ISaveLoaderSystem, UniTask<bool>> opGetter,
        string opName)
    {
        var saveTasks = new List<UniTask<bool>>(_saveLoadHandlers[loadGroup].Select(opGetter));
        await UniTask.WhenAll(saveTasks);
        bool allSucceeded = true;
        for (int i = 0; i < _saveLoadHandlers[loadGroup].Count; i++)
        {
            if (saveTasks[i].Status != UniTaskStatus.Succeeded)
            {
                _config.Log($"{opName} Failed for System: {_saveLoadHandlers[loadGroup][i].GetType().Name.ColoredRed()}");
                allSucceeded = false;
            }
        }
        return allSucceeded;
    }
}