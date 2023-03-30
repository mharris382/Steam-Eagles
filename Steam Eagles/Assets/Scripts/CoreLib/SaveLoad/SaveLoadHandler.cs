using System;
using System.Collections;
using CoreLib;
using CoreLib.SharedVariables;
using StateMachine;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using Task = System.Threading.Tasks.Task;
[System.Obsolete("Use Persistence Manager Instead")]
public class SaveLoadHandler : Singleton<SaveLoadHandler>
{
    
    public event Action<SaveSlot> OnSaveGame;
    public event Action<SaveSlot> OnLoadGame;


    private const string SHARED_TRANSFORM_TP = "Transform_Builder";
    private const string SHARED_TRANSFORM_BD = "Transform_Transporter";
    private const string DEFAULT_SAVE_SLOT = "SaveSlot_Default";
    

    public int startSceneIndex = 1;
    public SharedTransform SharedTransformTp;
    public SharedTransform SharedTransformBd;
    
    
    
    public SaveSlot DefaultSaveSlot;
    private SaveSlot _currentSaveSlot;
    public SaveSlot CurrentSaveSlot
    {
        get => _currentSaveSlot ? _currentSaveSlot : DefaultSaveSlot;
    }

    bool MissingResources()
    {
        return SharedTransformBd == null || SharedTransformTp == null;
    }

    IEnumerator LoadReferences()
    {
        var loadBd = Addressables.LoadAssetAsync<SharedTransform>(SHARED_TRANSFORM_BD);
        var loadTp = Addressables.LoadAssetAsync<SharedTransform>(SHARED_TRANSFORM_TP);
        var loadDefaultSaveSlot = Addressables.LoadAssetAsync<SaveSlot>(DEFAULT_SAVE_SLOT);
        yield return loadBd;
        SharedTransformBd = loadBd.Result;
        Debug.Assert(SharedTransformTp != null, $"No SharedTransform found for Builder at {SHARED_TRANSFORM_BD}");
        yield return loadTp;
        SharedTransformTp = loadTp.Result;
        Debug.Assert(SharedTransformTp != null, $"No SharedTransform found for Transporter at {SHARED_TRANSFORM_TP}");
        yield return loadDefaultSaveSlot;
        DefaultSaveSlot = loadDefaultSaveSlot.Result;
        Debug.Assert(DefaultSaveSlot != null, $"No SaveSlot Addressable Found at {DEFAULT_SAVE_SLOT}");
    }


    private IEnumerator Start()
    {
        
        yield return LoadReferences();
        if (MissingResources())
        {
            Debug.LogError("Missing Resources", this);
            yield break;
        }
    }

    public IEnumerator StartGame(SaveSlot saveSlot)
    {
        var saveData = saveSlot.LoadSharedSaveData();
        if (saveData.gameStarted)
        {
            //Starting a new game, so save scene info when scene loads for the first time
            yield return SceneManager.LoadSceneAsync(startSceneIndex);
        }
        else 
        {
            //game has been started so load the scene info after the scene loads
            yield return SceneManager.LoadSceneAsync(saveData.currentLevel);
            SharedTransformTp.RespawnPosition = saveData.transporterPosition;
            SharedTransformBd.RespawnPosition = saveData.builderPosition;
        }
    }

    public SaveSlot GetCurrentSaveSlot()
    {
        return CurrentSaveSlot;
    }
}