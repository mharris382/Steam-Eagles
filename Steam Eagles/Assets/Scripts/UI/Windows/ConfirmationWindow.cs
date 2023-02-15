using System;
using Sirenix.OdinInspector;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class ConfirmationWindow : Window
    {
        [Required, SerializeField] private TextMeshProUGUI promptText;
        [Required, SerializeField] private Button yesButton;
        [Required, SerializeField] private Button noButton;

        
        private CompositeDisposable _listeners;
        
        public void Awake()
        {
            SetWindowVisible(false);
        }



      
        public void SelectNo(EventSystem eventSystem)
        {
            eventSystem.SetSelectedGameObject(noButton.gameObject);
        }
        
        public void Show(string prompt, Action onYes, Action onNo)
        {
            promptText.text = prompt;
            if(_listeners != null)
                _listeners.Dispose();
            _listeners = new CompositeDisposable();
            AddYesListener(onYes);
            AddNoListener(onNo);
            SetWindowVisible(true);
        }
        
        private void SetPrompt(string prompt)
        {
            promptText.text = prompt;
        }
        
        private void AddYesListener(Action action)
        {
            yesButton.onClick.AsObservable().Subscribe(_ => action()).AddTo(_listeners);
        }
   
        
        private void AddNoListener(Action action)
        {
            noButton.onClick.AsObservable().Subscribe(_ => action()).AddTo(_listeners);
        }
        
        public void ClearListeners()
        {
            _listeners.Dispose();
            _listeners = null;
        }
    }
}
