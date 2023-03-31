using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using CoreLib;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class UIPromptBuilder : Singleton<UIPromptBuilder>
    {
        ConfirmationWindow _window;

        private List<UIPrompt> _prompts = new List<UIPrompt>();

        private IEnumerator Start()
        {
            while (!WindowLoader.Instance.IsFinishedLoading())
                yield return null;
            _window = WindowLoader.GetWindow<ConfirmationWindow>();
        }
        
        private UIPrompt _currentPrompt;
        
       
        public class UIPrompt
        {
            private readonly Transform _parent;
            private readonly ConfirmationWindow window;
            private readonly Func<string> message;
            private Func<Selectable> _getLastSelectable;
            private readonly EventSystem _eventSystem;
            
            public UIPrompt(
                Transform parent,
                Func<string> message,
                ConfirmationWindow window,
                Func<Selectable> getLastSelectable,
                EventSystem eventSystem)
            {
                _parent = parent;
                this.window = window;
                this._eventSystem = eventSystem;
                this.message = message;
                this._getLastSelectable = getLastSelectable;
            }
            public UIPrompt(
                Transform parent,
                string message,
                ConfirmationWindow window,
                Func<Selectable> getLastSelectable,
                EventSystem eventSystem) : this(parent,() => message, window, getLastSelectable, eventSystem)
            {
            }

            public void DisplayPrompt(Action yes, Action no = null)
            {
                if (no == null)
                {
                    no = () => { };
                }
                window.Show(message(), yes, no);
                window.SelectNo(_eventSystem);
                window.RectTransform.SetParent(_parent);
                window.RectTransform.anchoredPosition = Vector2.zero;
                window.SetWindowVisible(true);
            }

            public IEnumerator DisplayPromptStream(IObserver<bool> ob, CancellationToken ct)
            {
                bool finished = false;
                window.Show(message(), 
                    () =>
                    {
                        finished = true;
                        ob.OnNext(true);
                        ob.OnCompleted();
                        window.SetWindowVisible(false);
                    },
                    () =>
                    {
                        finished = true;
                        ob.OnNext(false);
                        ob.OnCompleted();
                        OnCancel();
                        window.SetWindowVisible(false);
                    });
                window.SelectNo(_eventSystem);
                window.RectTransform.SetParent(this._parent);
                window.RectTransform.anchoredPosition = Vector2.zero;
                window.SetWindowVisible(true);
                while (!window.IsVisible )
                {
                    if (ct.IsCancellationRequested)
                    {
                        ob.OnError(new Exception("Cancelled"));
                        yield break;
                    }
                    yield return null;
                }
                OnCancel();
            }
            
            public void OnCancel()
            {
                if (!window.IsVisible) return;
                window.ClearListeners();
                window.SetWindowVisible(false);
                var lastSelectable = _getLastSelectable();
                if (lastSelectable != null) _eventSystem.SetSelectedGameObject(lastSelectable.gameObject);
            }
        }

        ConfirmationWindow Show(Transform  promptParent)
        {
            _window = WindowLoader.GetWindow<ConfirmationWindow>();
            _window.transform.SetParent(promptParent);
            _window.RectTransform.anchoredPosition = Vector2.zero;
            _window.SetWindowVisible(true);
            return _window;
        }

        public ConfirmationPromptID CreatePrompt(Transform  promptParent, EventSystem eventSystem, Func<Selectable> lastSelected, string message)
        {
            int id = _prompts.Count;
            var prompt = new UIPrompt(promptParent, message, _window, lastSelected, eventSystem);
            _prompts.Add(prompt);
            return new ConfirmationPromptID()
            {
                id = id
            };
        }
        public ConfirmationPromptID CreatePrompt(Transform  promptParent, EventSystem eventSystem, Func<Selectable> lastSelected, Func<string> message)
        {
            int id = _prompts.Count;
            var prompt = new UIPrompt(promptParent, message, _window, lastSelected, eventSystem);
            _prompts.Add(prompt);
            return new ConfirmationPromptID()
            {
                id = id
            };
        }


        /// <summary>
        /// returns observable which emits true if yes was pressed, false if no was pressed
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IObservable<bool> ShowPrompt(ConfirmationPromptID id)
        {
            var prompt = _prompts[id.id];
            return Observable.FromCoroutine<bool>(prompt.DisplayPromptStream);
        }
    }
    
    
    
    public struct ConfirmationPromptID
    {
        public int id;

        public override int GetHashCode()
        {
            return id;
        }
    }
}