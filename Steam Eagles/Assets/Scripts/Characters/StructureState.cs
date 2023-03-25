using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Characters
{
    /// <summary>
    /// tracks information about the environment surrounding the character, particularly
    /// objects that the character can interact with or influence the character's movement
    /// </summary>
    public class StructureState : MonoBehaviour
    {
        [Range(1, 50)]
        [SerializeField] private int maxTriggerCount = 10;

        [SerializeField] private bool useOverlapArea = false;
        [SerializeField, ShowIf("useOverlapArea")] private Vector2 overlapAreaP0 = Vector2.up;
        [SerializeField, ShowIf("useOverlapArea")] private Vector2 overlapAreaP1 = Vector2.down;
        public enum JointMode
        {
            /// <summary>
            /// in this mode the joint is disabled, even if the character has a building
            /// </summary>
            DISABLED,
            /// <summary>
            /// in this mode the joint is always enabled, provided the character has a building
            /// </summary>
            ENABLED,
            /// <summary>
            /// in this mode the joint will be enabled if the character is grounded, and disabled otherwise
            /// </summary>
            AUTOMATIC
        }
        
        private FixedJoint2D _buildingJoint;
        private CharacterState _state;
        private BoolReactiveProperty _isJointEnabled = new BoolReactiveProperty(false);
        private ReactiveProperty<Rigidbody2D> _buildingRigidbody = new ReactiveProperty<Rigidbody2D>();
        private ReactiveProperty<Rigidbody2D> _platformRigidbody = new ReactiveProperty<Rigidbody2D>();
        private BoolReactiveProperty _hasLadder = new BoolReactiveProperty(false);
        private ReactiveProperty<JointMode> _jointMode = new ReactiveProperty<JointMode>();
        private CharacterState _characterState;
        private LayerMask _buildingLayerMask;
        private int _triggerHits = 0;
        private Collider2D[] _triggerColliders;
        
        private Coroutine _autoJointCoroutine;
        private IDisposable _enableJointDisposable;
        public FixedJoint2D BuildingJoint => _buildingJoint;

        [ShowInInspector]
        public Rigidbody2D BuildingRigidbody
        {
            get => _buildingRigidbody.Value;
            set => _buildingRigidbody.Value = value;
        }

        [ShowInInspector]
        public Rigidbody2D PlatformRigidbody
        {
            get => _platformRigidbody.Value;
            set => _platformRigidbody.Value = value;
        }
        
        public bool HasLadder
        {
            get => _hasLadder.Value;
            set => _hasLadder.Value = value;
        }
        private ReactiveProperty<JointMode> JointModeProperty => _jointMode ??= new ReactiveProperty<JointMode>(JointMode.DISABLED);
        [ShowInInspector]
        public JointMode Mode
        {
            get => JointModeProperty.Value; 
            set => JointModeProperty.Value = value;
        }

        public IEnumerable<Collider2D> GetOverlappingColliders()
        {
            if(_triggerHits <= 0)
                yield break;
            for (int i = 0; i < _triggerHits; i++)
            {
                yield return _triggerColliders[i];
            }
        }

        [ShowInInspector]
        private Rigidbody2D ConnectedBody => PlatformRigidbody == null ? BuildingRigidbody : PlatformRigidbody;

        [ShowInInspector]
        public bool IsGrounded => _characterState.IsGrounded;

        private void Awake()
        {
            _triggerColliders = new Collider2D[maxTriggerCount];
            _buildingJoint = GetComponent<FixedJoint2D>();
            _characterState = GetComponent<CharacterState>();
            _buildingJoint.autoConfigureConnectedAnchor = false;
            _buildingJoint.enabled = false;
            _isJointEnabled = new BoolReactiveProperty(false);
            _isJointEnabled.Subscribe(jointEnabled =>
            {
                if (jointEnabled)
                {
                    Debug.Assert(ConnectedBody != null, "Why the hell are you enabling the joint without a connected body?");
                    BuildingJoint.connectedAnchor = ConnectedBody.transform.InverseTransformPoint(transform.position);
                }
                BuildingJoint.enabled = jointEnabled;
            }).AddTo(this);
            _characterState = GetComponent<CharacterState>();
            _buildingLayerMask = LayerMask.GetMask("Triggers");
            
            JointModeProperty.Subscribe(mode =>
            {
                switch (mode)
                {
                    case JointMode.DISABLED:
                        Debug.Log("disabled joint mode",this);
                        
                        CancelJointCoroutine();
                        DisposeEnableObserver();
                        _buildingJoint.enabled = false;
                        
                        break;
                    case JointMode.ENABLED:
                        Debug.Log("enabled joint mode", this);
                        
                        //cancel any auto joint coroutine
                        CancelJointCoroutine();
                        SubscribeEnableObserver();
                        
                        break;
                    case JointMode.AUTOMATIC:
                        Debug.Log("automatic joint mode", this);
                        
                        DisposeEnableObserver();
                       _autoJointCoroutine = StartCoroutine(AutoJointMode());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
                }
            });
        }



        private void DisposeEnableObserver()
        {
            _enableJointDisposable?.Dispose();
            _enableJointDisposable = null;
        }
        
        private void SubscribeEnableObserver()
        {
            _enableJointDisposable = _buildingRigidbody.Merge(_platformRigidbody)
                .Select(_ => ConnectedBody).Subscribe(body =>
                {
                    if (body != null)
                    {
                        _buildingJoint.connectedBody = body;
                        _buildingJoint.autoConfigureConnectedAnchor = true;
                        _isJointEnabled.Value = true;
                    }
                    else
                    {
                        _buildingJoint.enabled = false;
                    }
                });
            _autoJointCoroutine = StartCoroutine(EnableJointMode());
        }

        private void CancelJointCoroutine()
        {
            if (_autoJointCoroutine != null)
            {
                StopCoroutine(_autoJointCoroutine);
                _autoJointCoroutine = null;
            }
        }

        private void Update()
        {
            if (_characterState.IsDropping)
            {
                _buildingJoint.enabled = false;
                return;
            }
            if (Mode == JointMode.ENABLED )
            {
                _buildingJoint.enabled = (_isJointEnabled.Value);
            }
        }

        public void CheckForStructures()
        {
            var pos = transform.position;
            var prev = Physics2D.queriesHitTriggers;
            Physics2D.queriesHitTriggers = true;
            
            if (useOverlapArea)
            {
                _triggerHits = Physics2D.OverlapAreaNonAlloc(OverlapAreaP0, OverlapAreaP1, _triggerColliders, _buildingLayerMask);
            }
            else
            {
                _triggerHits = Physics2D.OverlapPointNonAlloc(pos, _triggerColliders, _buildingLayerMask);
            }
            bool foundBuilding = false;
            bool foundPlatform = false;
            bool foundLadder = false;
            for (int i = 0; i < _triggerHits; i++)
            {
                var coll = _triggerColliders[i];
                if (coll.gameObject.CompareTag("Building"))
                {
                    foundBuilding = true;
                    BuildingRigidbody = coll.attachedRigidbody;
                }
                else if (coll.gameObject.CompareTag("Moving Platform"))
                {
                    foundPlatform = true;
                    PlatformRigidbody = coll.attachedRigidbody;
                }
                else if (LadderCheck.IsColliderLadder(coll))
                {
                    foundLadder = true;
                }
            }
            if(!foundBuilding) BuildingRigidbody = null;
            if(!foundPlatform) PlatformRigidbody = null;
            Physics2D.queriesHitTriggers = prev;
        }
        
        private IEnumerator AutoJointMode()
        {
            while (_jointMode.Value == JointMode.AUTOMATIC)
            {
                if (IsGrounded)
                {
                    _buildingJoint.connectedBody = ConnectedBody;
                    _isJointEnabled.Value= _buildingJoint.connectedBody != null;
                }
                else
                {
                    _buildingJoint.enabled = false;
                }
                yield return null;
            }
        }

        private IEnumerator EnableJointMode()
        {
            while (_jointMode.Value == JointMode.ENABLED)
            {
                _isJointEnabled.Value = _buildingJoint.connectedBody != null;
                yield return null;
            }
        }

        
        
        private void OnDrawGizmosSelected()
        {
            if (useOverlapArea)
            {
                var p0 = OverlapAreaP0;
                var p1 = OverlapAreaP1;
                Gizmos.color = Color.red;
                Gizmos.DrawLine(p0, p1);
            }
        }

        private Vector2 OverlapAreaP1 => (Vector2)transform.position + overlapAreaP1;

        private Vector2 OverlapAreaP0 => (Vector2)transform.position + overlapAreaP0;
    }



    
    public class LadderCheck
    {
        public static bool IsColliderLadder(Collider2D collider) => collider.gameObject.CompareTag("Ladder");
    }
}
