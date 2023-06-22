using System;
using System.Collections;
using System.Collections.Generic;
using CoreLib;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

namespace UI
{
    public class WindowLoader : MonoBehaviour
    {
        
        public string confirmationWindowAssetAddress = "Confirmation Window";
        public string infoBoxWindowAssetAddress = "Info Box Window";
        public string tooltipWindowAssetAddress = "Tooltip Window";

        private AsyncOperationHandle<GameObject> _confirmationWindowLoadOp;
        private AsyncOperationHandle<GameObject> _infoBoxWindowLoadOp;
        private AsyncOperationHandle<GameObject> _tooltipWindowLoadOp;
        
        private GameObject _confirmationWindowPrefab;
        private GameObject _infoBoxWindowPrefab;
        private GameObject _tooltipWindowPrefab;
        
        private ConfirmationWindow[] _confirmationWindowInstance;
        private UIInfoBoxWindow[] _infoBoxWindowInstance;
        private TooltipWindow[] _tooltipWindowInstance;
        
        private DiContainer _container;

        
        [Inject] void InjectMe(DiContainer container)
        {
            _container = container;
        }
        
        
        
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
            _confirmationWindowInstance = new ConfirmationWindow[2];
            _infoBoxWindowInstance = new UIInfoBoxWindow[2];
            _tooltipWindowInstance = new TooltipWindow[2];
            yield return UniTask.ToCoroutine(async () =>
            {
                _infoBoxWindowLoadOp = Addressables.LoadAssetAsync<GameObject>(infoBoxWindowAssetAddress);
                _confirmationWindowLoadOp = Addressables.LoadAssetAsync<GameObject>(confirmationWindowAssetAddress);
                _tooltipWindowLoadOp = Addressables.LoadAssetAsync<GameObject>(tooltipWindowAssetAddress);
                await _tooltipWindowLoadOp.ToUniTask();
                await _confirmationWindowLoadOp.ToUniTask();
                await _infoBoxWindowLoadOp.ToUniTask();
                _infoBoxWindowPrefab = _infoBoxWindowLoadOp.Result;
                _confirmationWindowPrefab = _confirmationWindowLoadOp.Result;
                _tooltipWindowPrefab = _tooltipWindowLoadOp.Result;
            });
            // LoadConfirmationWindow();
            // LoadInfoBoxWindow();
            // yield return _confirmationWindowLoadOp;
            // _confirmationWindowPrefab = _confirmationWindowLoadOp.Result;
            // yield return _infoBoxWindowLoadOp;
            // _infoBoxWindowPrefab = _infoBoxWindowLoadOp.Result;
            //
        }
        public bool IsFinishedLoading()
        {
            return _confirmationWindowPrefab != null && 
                   _infoBoxWindowPrefab != null;
        }
        public T GetWindow<T>() where T: class
        {
            return GetWindowInternal<T>(0);
        }
        private T GetWindowInternal<T>(int i) where  T: class
        {
            if (!IsFinishedLoading())
            {
                Debug.LogError("WindowManager is not finished loading");
                return null;
            }
            switch (typeof(T))
            {
                case { } confirmationWindowType when confirmationWindowType == typeof(ConfirmationWindow):
                    if (_confirmationWindowInstance[i] == null)
                    {
                        var instance = Instantiate(_confirmationWindowPrefab);
                        _confirmationWindowInstance[i] = instance.GetComponent<ConfirmationWindow>();
                        Debug.Assert(_confirmationWindowInstance[i] != null, "confirmationWindow prefab is missing confirmation window component");
                        _container.InjectGameObject(_confirmationWindowInstance[i].gameObject);
                    }
                    return _confirmationWindowInstance[i] as T;
                
                case { }  itemTooltipWindowType when itemTooltipWindowType == typeof(TooltipWindow):
                    if (_tooltipWindowInstance[i] == null)
                    {
                        var instance = Instantiate(_tooltipWindowPrefab);
                        _tooltipWindowInstance[i] = instance.GetComponent<TooltipWindow>();
                        Debug.Assert(_tooltipWindowInstance[i] != null, "tooltipWindow prefab is missing tooltip window component");
                        _container.InjectGameObject(_tooltipWindowInstance[i].gameObject);
                    }
                    return _tooltipWindowInstance[i] as T;
                case { } infoBoxWindowType when infoBoxWindowType == typeof(UIInfoBoxWindow):
                    if (_infoBoxWindowInstance[i] == null)
                    {
                        var instance = Instantiate(_infoBoxWindowPrefab);
                        _infoBoxWindowInstance[i] = instance.GetComponent<UIInfoBoxWindow>();
                        Debug.Assert(_infoBoxWindowInstance[i] != null, "infoBoxWindow prefab is missing info box window component");
                        _container.InjectGameObject(_infoBoxWindowInstance[i].gameObject);
                    }
                    return _infoBoxWindowInstance as T;
                
                
                default:
                    Debug.LogError($"Window of type {typeof(T)} not found", this);
                    throw new InvalidOperationException("Window type not supported");
            }
        }
    }
}