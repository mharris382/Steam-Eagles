using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public bool autoLoadScene = true;
    
    public LoadSceneMode mode =LoadSceneMode.Additive;
    
    public bool loadByName = false;


    [HideIf(nameof(loadByName))]
    public int autoLoadSceneID = 1;

    private AsyncOperation _loadOp;


    [ShowIf(nameof(loadByName))]
    public string sceneName;

    public UnityEvent onFinishedLoad;
    public UnityEvent onStartLoad;

    public void Awake()
    {
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

        if (loadByName)
        {
            this._loadOp = SceneManager.LoadSceneAsync(sceneName, mode);
            StartCoroutine(Load());
        }

        else
        {
            this._loadOp = SceneManager.LoadSceneAsync(autoLoadSceneID, mode);
            StartCoroutine(Load());
        }
    }

    IEnumerator Load()
    {
        onStartLoad?.Invoke();
        yield return _loadOp;
        onFinishedLoad?.Invoke();
    }
}
