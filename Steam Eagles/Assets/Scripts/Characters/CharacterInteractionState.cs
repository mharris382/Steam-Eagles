using System;
using UniRx;
using UnityEngine;

namespace Characters
{
    public class CharacterInteractionState : MonoBehaviour
    {
        public class Inputs
        {
            public bool InteractPressed { get; set; }
            public bool CancelPressed { get; set; }
            public int InteractDirectionY { get; set; }
            public Vector2 InteractAimDirection { get; set; } 
        }

        private BoolReactiveProperty _isInteracting = new BoolReactiveProperty();
        private Inputs _inputs;

        public bool IsInteracting
        {
            get => _isInteracting.Value;
            set => _isInteracting.Value = value;
        }

        public IObservable<bool> IsInteractingObservable => _isInteracting ??= new BoolReactiveProperty();

        public Inputs Input => _inputs ??= new Inputs();
    }
}