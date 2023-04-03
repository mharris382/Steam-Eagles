using System;
using System.Collections;
using CoreLib;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UI
{
    public class WindowLoader : Singleton<WindowLoader>
    {
        public override bool DestroyOnLoad => false;
        public string confirmationWindowAssetAddress = "Confirmation Window";
        public string infoBoxWindowAssetAddress = "Info Box Window";


        private GameObject _confirmationWindowPrefab;
        private GameObject _infoBoxWindowPrefab;
        private ConfirmationWindow _confirmationWindowInstance;
        private UIInfoBoxWindow _infoBoxWindowInstance;
        
        private AsyncOperationHandle<GameObject> _confirmationWindowLoadOp;
        private AsyncOperationHandle<GameObject> _infoBoxWindowLoadOp;

        
        public class LocalPlayerUIWindows
        {
            public UICharacterInventoryWindow _uiCharacterInventoryWindow;
            public UICharacterToolBeltHUD uiCharacterToolBeltHUD;
        }
        
        private void LoadConfirmationWindow()
        {
            _confirmationWindowLoadOp = Addressables.LoadAssetAsync<GameObject>(confirmationWindowAssetAddress);
        }

        private void LoadInfoBoxWindow()
        {
            _infoBoxWindowLoadOp = Addressables.LoadAssetAsync<GameObject>(infoBoxWindowAssetAddress);
        }
        private IEnumerator Start()
        {
            LoadConfirmationWindow();
            LoadInfoBoxWindow();
            yield return _confirmationWindowLoadOp;
            _confirmationWindowPrefab = _confirmationWindowLoadOp.Result;
            yield return _infoBoxWindowLoadOp;
            _infoBoxWindowPrefab = _infoBoxWindowLoadOp.Result;
        }
        public bool IsFinishedLoading()
        {
            return _confirmationWindowPrefab != null && 
                   _infoBoxWindowPrefab != null;
        }
        public static T GetWindow<T>() where T : Window
        {
            return Instance.GetWindowInternal<T>();
        }
        private T GetWindowInternal<T>() where T : Window
        {
            if (!IsFinishedLoading())
            {
                Debug.LogError("WindowManager is not finished loading");
                return null;
            }
            switch (typeof(T))
            {
                case { } confirmationWindowType when confirmationWindowType == typeof(ConfirmationWindow):
                    if (_confirmationWindowInstance == null)
                    {
                        var instance = Instantiate(_confirmationWindowPrefab);
                        _confirmationWindowInstance = instance.GetComponent<ConfirmationWindow>();
                        Debug.Assert(_confirmationWindowInstance != null, "confirmationWindow prefab is missing confirmation window component");
                    }
                    return _confirmationWindowInstance as T;
                
                
                case { } infoBoxWindowType when infoBoxWindowType == typeof(UIInfoBoxWindow):
                    if (_infoBoxWindowInstance == null)
                    {
                        var instance = Instantiate(_infoBoxWindowPrefab);
                        _infoBoxWindowInstance = instance.GetComponent<UIInfoBoxWindow>();
                        Debug.Assert(_infoBoxWindowInstance != null, "infoBoxWindow prefab is missing info box window component");
                    }
                    return _infoBoxWindowInstance as T;
                
                
                default:
                    Debug.LogError($"Window of type {typeof(T)} not found", this);
                    throw new InvalidOperationException("Window type not supported");
            }
        }
    }
}