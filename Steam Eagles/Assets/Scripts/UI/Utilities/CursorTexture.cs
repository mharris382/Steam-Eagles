using System;
using System.Collections.Generic;
using CoreLib;
using UniRx;
using UnityEngine;

namespace UI.Utilities
{
    /// <summary>
    /// used to update the cursor texture in different states
    /// </summary>
    public class CursorTexture : MonoBehaviour
    {
        [Serializable]
        public class CursorStateSettings
        {
            public string stateName = "New Cursor State";
            public bool visible;
            public Texture2D texture;
            public CursorLockMode lockMode = CursorLockMode.None;
            public Vector2 hotspot;
            public CursorMode mode = CursorMode.Auto;
            [Header("Cursor State")] 
            public SharedBool isStateActive;
            public void UpdateCursor()
            {
                Cursor.visible = visible;
                Cursor.lockState = lockMode;
                if (visible && texture != null)
                {
                    Cursor.SetCursor(texture, hotspot, mode);
                }
            }

        
            public void Setup(CursorTexture cursorTexture)
            {
                if (isStateActive == null) return;
                isStateActive.onValueChanged.AsObservable().Where(t => t).Subscribe(_ =>
                {
                    cursorTexture.stateStack.Push(this);
                    cursorTexture.NotifyStateChange();
                }).AddTo(cursorTexture);
                isStateActive.onValueChanged.AsObservable().Where(t => !t).Subscribe(_ =>
                {
                    if (cursorTexture.stateStack.Peek() == this)
                    {
                        cursorTexture.stateStack.Pop();
                        cursorTexture.NotifyStateChange();
                    }
                }).AddTo(cursorTexture);
            }
        }

        private void NotifyStateChange()
        {
            if (stateStack.Count == 0)
            {
                stateStack.Push(defaultState);
            }
            stateStack.Peek().UpdateCursor();
        }

        public CursorStateSettings defaultState;
        [SerializeField] public CursorStateSettings[] states;
    


    
        private Stack<CursorStateSettings> stateStack = new Stack<CursorStateSettings>();

        private void Awake()
        {
        
            foreach (var state in states)
            {
                var cursorState = state;
                cursorState.Setup(this);
            }

            stateStack.Push(defaultState);
            NotifyStateChange();
        }
    
    }
}

