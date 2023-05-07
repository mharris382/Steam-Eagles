using System;
using System.Collections.Generic;
using CoreLib;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Interactions
{
    
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(SliderJoint2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class ElevatorController : MonoBehaviour
    {
        public bool flipInput = true;
        public BoxCollider2D floorCollider;
        private SliderJoint2D _sliderJoint2D;
        private BoxCollider2D _boxCollider2D;
        private Rigidbody2D _rigidbody2D;
        private IElevatorMechanism _elevatorMechanism;
        
        public SliderJoint2D SliderJoint2D => _sliderJoint2D ??= GetComponent<SliderJoint2D>();
        public BoxCollider2D BoxCollider2D => _boxCollider2D ??= GetComponent<BoxCollider2D>();
        public Rigidbody2D Rigidbody2D => _rigidbody2D ??= GetComponent<Rigidbody2D>();
        public bool IsMoving => _elevatorMechanism.IsMoving;


        [Inject]
        public void InjectMe(IElevatorMechanism elevatorMechanism)
        {
            _elevatorMechanism = elevatorMechanism;
            testers = new List<ButtonTesters>();
            int cnt = 0;
            foreach (var floorName in _elevatorMechanism.GetFloorNames())
            {
                testers.Add(new ButtonTesters()
                {
                    button = cnt,
                    floorName = floorName,
                    elevatorMechanism = elevatorMechanism
                });
                cnt++;
            }
        }

        [SerializeField, ShowInInspector, HideInEditorMode]
        private List<ButtonTesters> testers;

        [Serializable]
        class ButtonTesters
        {
            public int button;
            public string floorName;
            public IElevatorMechanism elevatorMechanism;

            bool CanPressButton()
            {
                return elevatorMechanism != null && elevatorMechanism.IsMoving == false;
            }
            [Button, EnableIf(nameof(CanPressButton))]
            public void Test()
            {
                elevatorMechanism.MoveToFloor(button);
            }
        }
        
        
        
    }
}