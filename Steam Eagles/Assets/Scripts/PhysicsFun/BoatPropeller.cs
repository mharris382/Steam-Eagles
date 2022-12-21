using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace PhysicsFun
{
    [RequireComponent(typeof(ConstantForce2D))]
    public class BoatPropeller : MonoBehaviour
    {
        private ConstantForce2D _constantForce2D;

        private ConstantForce2D ConstantForce2D =>
            _constantForce2D ? _constantForce2D : _constantForce2D = GetComponent<ConstantForce2D>();
    
        
        [SerializeField] private float propellerRadius = 0.5f;
        [SerializeField] private float propellerForwardsForce = 10f;
        [SerializeField] private float propellerReverseForce = 10f;
    
        [SerializeField] private IntReactiveProperty _direction = new IntReactiveProperty(1);
    
        [SerializeField] private Events events;
    
        private FloatReactiveProperty _propellerSpeed = new FloatReactiveProperty(0f);
        private BoolReactiveProperty _isPowered = new BoolReactiveProperty();
        private BoolReactiveProperty _inWater = new BoolReactiveProperty();
    
    
    
        public bool InWater
        {
            private set => _inWater.Value = value;
            get => _inWater.Value;
        }
    
        public bool IsPowered
        {
            get => _isPowered.Value;
            set => _isPowered.Value = value;
        }

        public int Direction
        {
            get => _direction.Value;
            set => _direction.Value = Mathf.Clamp(value, -1, 1);
        }

        public bool IsFunctional => InWater && IsPowered;

        private void Awake()
        {
            _isPowered = new BoolReactiveProperty();
            _inWater = new BoolReactiveProperty();
            _direction = new IntReactiveProperty(1);
            var isPoweredAndOn = _isPowered.CombineLatest(_direction, (isPowered, direction) => isPowered && direction != 0);
        
            var isConstantForceEnabled = isPoweredAndOn.CombineLatest(_inWater, (isPowAndOn, inWater) => isPowAndOn && inWater)
                .Subscribe(isEnabled => ConstantForce2D.enabled = isEnabled)
                .AddTo(this);
        
            var propellerForce = isPoweredAndOn.CombineLatest(_inWater, (isPowAndOn, inWater) => isPowAndOn && inWater)
                .Select(t => t ? Direction * (Direction > 0 ? propellerForwardsForce : propellerReverseForce) : 0f)
                .Subscribe(force => ConstantForce2D.force = new Vector2(force, 0))
                .AddTo(this);
        
            _inWater.CombineLatest(isPoweredAndOn, (sub, pow) => (sub,pow))
                .Subscribe(t => events.RaiseEvents(t.pow, t.sub, Direction))
                .AddTo(this);
        }

        private void Update()
        {
            InWater = IsPropellerSubmerged();
        }

        bool IsPropellerSubmerged()
        {
            LayerMask mask = LayerMask.GetMask("Water");
            var pos = transform.position;
            var coll = Physics2D.OverlapCircle(pos, propellerRadius, mask);
            return coll != null;
        }
    
        static Color submergedActive = Color.green;
        static Color inactive  = Color.red; 
        static Color activeNoSubmerged = Color.magenta;
    
        private void OnDrawGizmos()
        {
            bool isFunctional = false;
            bool inWater = false;
            bool isPowered = false;
            if (Application.isPlaying)
            {
                isFunctional = IsFunctional;
                inWater = InWater;
                isPowered = IsPowered;
            }
            else
            {
                inWater = isFunctional = IsPropellerSubmerged();
                isPowered = true;
            }
            Color color = submergedActive;
            if (!isFunctional)
            {
                color = isPowered ? inactive : activeNoSubmerged;
            }
            Gizmos.color = color;
            Gizmos.DrawSphere(transform.position, propellerRadius);
        }

    
    
    
        [Serializable]
        private class Events
        {
            [Header("Power")]
            public UnityEvent onPropellerPowered;
            public UnityEvent onPropellerUnpowered;

            [Header("Water")] 
            public UnityEvent onSubmerged;
            public UnityEvent onSurfaced;

            [Header("Functional")]
            public UnityEvent onPropellerFunctional;
            public UnityEvent onPropellerNonFunctional;

            [Header("Direction")]
            public UnityEvent<int> onDirectionChanged;
        
            void RaiseWaterEvents(bool inWater)
            {
                if (inWater)
                {
                    onSubmerged.Invoke();
                }
                else
                {
                    onSurfaced.Invoke();
                }
            }
            void RaisePowerEvents(bool powered)
            {
                if (!powered)
                {
                    onPropellerUnpowered?.Invoke();
                }
                else
                {
                    onPropellerPowered?.Invoke();
                }
            }
        
            public void RaiseEvents(bool powered, bool inWater, int direction)
            {
                RaisePowerEvents(powered);
                RaiseWaterEvents(inWater);
                onDirectionChanged?.Invoke(direction);
                if (inWater && powered && direction != 0)
                {
                    onPropellerFunctional?.Invoke();
                }
                else
                {
                    onPropellerNonFunctional?.Invoke();
                }
            }
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(BoatPropeller))]
    public class BoatPropellerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
        
            if (Application.isPlaying)
            {
                EditorGUILayout.BeginHorizontal();
                if(GUILayout.Button("Toggle Power")) ((BoatPropeller) target).IsPowered = !((BoatPropeller) target).IsPowered;
            
                if (GUILayout.Button("<", GUILayout.Width(25))) ((BoatPropeller)target).Direction = -1;
                if (GUILayout.Button("|", GUILayout.Width(25))) ((BoatPropeller)target).Direction = 0;
                if (GUILayout.Button(">", GUILayout.Width(25))) ((BoatPropeller)target).Direction = 1;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space(10);
            base.OnInspectorGUI();
        }
    }
#endif
}