using System;
using System.Collections;
using CoreLib;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UI
{
    public class WindowManager : Singleton<WindowManager>
    {
        public string confirmationWindowAssetAddress = "Confirmation Window";



        private GameObject _confirmationWindowPrefab;
        private ConfirmationWindow _confirmationWindowInstance;
        private AsyncOperationHandle<GameObject> _confirmationWindowLoadOp;

        private void LoadConfirmationWindow()
        {
            _confirmationWindowLoadOp = Addressables.LoadAssetAsync<GameObject>(confirmationWindowAssetAddress);
        }
        private IEnumerator Start()
        {
            LoadConfirmationWindow();
            yield return _confirmationWindowLoadOp;
            _confirmationWindowPrefab = _confirmationWindowLoadOp.Result;
        }
        public bool IsFinishedLoading()
        {
            return _confirmationWindowPrefab != null;
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
                default:
                    Debug.LogError($"Window of type {typeof(T)} not found", this);
                    throw new InvalidOperationException("Window type not supported");
            }
        }
    }
}