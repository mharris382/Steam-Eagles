using System;
using UnityEngine.Events;

namespace Tools.BuildTool
{
    [Serializable]
    public class ActivationEvents
    {
        public UnityEvent onActivated;
        public UnityEvent onDeactivated;
        public UnityEvent<bool> onActivationStateChanged;
        public void OnActivationStateChanged(bool active)
        {
            onActivationStateChanged.Invoke(active);
            if (active)
                onActivated.Invoke();
            else
                onDeactivated.Invoke();
        }
    }
}