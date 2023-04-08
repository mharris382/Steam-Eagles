using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI
{
    public static class SelectionMemory
    {
        static Dictionary<Window, Dictionary<PlayerInput, GameObject>> _lastSelectedByWindow = new Dictionary<Window, Dictionary<PlayerInput, GameObject>>();


        public static bool HasWindowBeenOpenedBy(this Window window, PlayerInput playerInput)
        {
            if (_lastSelectedByWindow.ContainsKey(window) == false)
            {
                _lastSelectedByWindow.Add(window, new Dictionary<PlayerInput, GameObject>());
                return false;
            }
            return _lastSelectedByWindow[window].ContainsKey(playerInput);
        }

        public static GameObject GetLastSelectable(this Window window, PlayerInput playerInput, GameObject fallback = null)
        {
            if (HasWindowBeenOpenedBy(window, playerInput) == false)
            {
                return fallback;
            }
            return _lastSelectedByWindow[window][playerInput];
        }
         
        public static void SetLastSelectable(this Window window, PlayerInput playerInput, GameObject selectable)
        {
            if (_lastSelectedByWindow.ContainsKey(window) == false)
            {
                _lastSelectedByWindow.Add(window, new Dictionary<PlayerInput, GameObject>());
            }
            _lastSelectedByWindow[window][playerInput] = selectable;
        }
    }
}