using System;
using StateMachine;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Game
{
    [RequireComponent(typeof(Collider2D))]
    public class AreaTrigger : MonoBehaviour
    {
        public SharedTransform p1Transform;
        public SharedTransform p2Transform;

         public BoolReactiveProperty player1InArea;
         public BoolReactiveProperty player2InArea;
         
         
         
        private bool p1InArea
        {
            get => player1InArea.Value;
            set => player1InArea.Value = value;
        }
        
        private bool p2InArea
        {
            get => player2InArea.Value;
            set => player2InArea.Value = value;
        }



        public UnityEvent<bool> onBothPlayersInArea;


        private void Awake()
        {
            
            player1InArea.ZipLatest(player2InArea, (b1, b2) => b1 && b2)
                .Subscribe(both => onBothPlayersInArea?.Invoke(both));
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            var attachedRigidbody = col.attachedRigidbody;
            if (attachedRigidbody == null) return;
            var attachedTransform = attachedRigidbody.transform;
            p1InArea = attachedTransform == p1Transform.Value;
            p2InArea = attachedTransform == p2Transform.Value;
        }
    }
}