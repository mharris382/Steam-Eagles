using System;
using System.Collections;
using System.Collections.Generic;
using Buildings;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Tools.DestructTool.Helpers
{
    public class DestructionController : MonoBehaviour
    {
        public float cellDestructionDelay = 0.1f;
        
        [SerializeField, Required] private Transform destructionPivot;
        [SerializeField] private Vector2 ellipseRadius = new Vector2(1, 1);
        [SerializeField] private LayerMask blockingLayer;
        [SerializeField] private  float movementSmoothing = 0.1f;
        
        private DestructionCollider _collider;
        private DestructionFeedbacks _feedbacks;
        private Coroutine _destructCoroutine;
        private Subject<(BuildingMap map, IEnumerable<BuildingCell>)> _cellsHit = new();
        
        private Vector3 _velocity;
        
        private void Awake()
        {
            _collider = GetComponentInChildren<DestructionCollider>();
            _feedbacks = GetComponentInChildren<DestructionFeedbacks>();
            Debug.Assert(_collider != null, "Destruction Collider is null", this);
            Debug.Assert(_feedbacks != null, "Destruction Visuals is null", this);
        }

        private void Start()
        {
            var hitStream = _cellsHit.Where(_ => enabled)
                .Buffer(TimeSpan.FromSeconds(cellDestructionDelay));
            
            hitStream
                .Where(_ => _.Count > 0)
                .Select(_ => _[0])
                .Subscribe(t => TryDestruct(t.map, t.Item2)).AddTo(this);
            
            hitStream.Select(_ => _.Count).Subscribe(_feedbacks.SetHitCount).AddTo(this);
            hitStream.Where(_ => _.Count > 0).Subscribe(_ => _feedbacks.SetHitTrigger());
        }

        private void OnEnable()
        {
            _collider.enabled = true;
            _feedbacks.SetToolInUseState(true);
            _collider.gameObject.SetActive(true);
        }
        private void OnDisable()
        {
            _collider.enabled = false;
            _feedbacks.SetToolInUseState(false);
            if(_destructCoroutine != null)StopCoroutine(_destructCoroutine);
        }

        

        public void UpdateAim(Vector2 direction)
        {
            var localScale = new Vector3(1, direction.x < 0 ? -1 : 1, 1);
            destructionPivot.localScale = localScale;
            Vector3 centerPosition = destructionPivot.position;
            float a = ellipseRadius.x;
            float b = ellipseRadius.y;
            Vector3 ellipsePosition = centerPosition + new Vector3(direction.x * a, direction.y * b);
            Vector3 targetPosition = ellipsePosition;
            RaycastHit2D hit= Physics2D.Linecast(centerPosition, ellipsePosition, blockingLayer);
            if (hit)
            {
                targetPosition = hit.point + (hit.normal * _collider.PlacementRadius);   
            }
            Vector3 actualPosition = Vector3.SmoothDamp(_collider.transform.position, targetPosition, ref _velocity, movementSmoothing );
            _collider.transform.position = actualPosition;
            HandleDestruction();
        }

        private void HandleDestruction()
        {
            
        }

        public void HandleBuildingDestruction(BuildingMap buildingMap)
        {
            _cellsHit.OnNext((buildingMap, _collider.GetEffectedCells(buildingMap)));
        }

        private void TryDestruct(BuildingMap map, IEnumerable<BuildingCell> cells)
        {
            foreach (var cell in cells)
            {
                map.DestructTile(cell);
            }
        }
    }
}