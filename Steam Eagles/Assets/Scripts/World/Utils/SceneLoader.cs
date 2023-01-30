using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public bool autoLoadScene = true;
    public int autoLoadSceneID = 1;
    private AsyncOperation _loadOp;

    public void Awake()
    {

        if (SceneManager.sceneCount > 1) return;
        if (autoLoadScene)
        {
            TriggerLoad();
        }
    }

    public void TriggerLoad()
    {
        
            
        if (_loadOp != null)
        {
            return;
        }
        
        this._loadOp = SceneManager.LoadSceneAsync(autoLoadSceneID, LoadSceneMode.Additive);
    }
}
