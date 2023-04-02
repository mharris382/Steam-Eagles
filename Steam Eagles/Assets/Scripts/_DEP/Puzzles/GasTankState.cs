using System;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Puzzles
{
    [RequireComponent(typeof(HoldableItem))]
    public class GasTankState : MonoBehaviour
    {
        public bool IsConnected
        {
            get;
            set;
        }

        public BoolReactiveProperty isConnected;
            public Events events;
            [Serializable]
            public class Events
            {
                public UnityEvent onBecameConnected;
                public UnityEvent onBecameDisconnected;
            }

            public bool debugConnected;
            private void Awake()
            {
                isConnected.TakeUntilDestroy(this).Subscribe(isConnected =>
                {
                    if (isConnected)
                    {
                        events.onBecameConnected?.Invoke();
                    }
                    else
                    {
                        events.onBecameDisconnected?.Invoke();
                    }
                });
            }


            private void Update()
            {
                if (debugConnected)
                {
                    isConnected.Value = !isConnected.Value;
                    debugConnected = false;
                }
            }
    }
}