using System;
using UnityEngine;

namespace CoreLib
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T _instance;
        public static bool HasInstance => _instance != null;
        public  static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        Debug.LogWarning($"No Singleton: {typeof(T).Name} found. Creating one now");
                        var go = new GameObject(typeof(T).Name.ToUpper(), typeof(T));
                        _instance = go.GetComponent<T>();
                        _instance.OnCreatedFromScript();
                    }
                }
                return _instance;
            }
        }
        
        public abstract bool DestroyOnLoad { get; }

        protected virtual void OnCreatedFromScript()
        {
            
        }

        private void Awake()
        {
            if (_instance == null || _instance == this)
            {
                _instance = this as T;
                if(!DestroyOnLoad)
                    DontDestroyOnLoad(_instance);
                Init();
            }
            else
            {
                Debug.LogWarning($"Found multiple {typeof(T).Name} found in scene. Deleting extra");
                CleanupDuplicate();
                Destroy(this);
            }
        }

        protected virtual void CleanupDuplicate()
        {
            
        }
        protected virtual void Init()
        {
        }
    }
}