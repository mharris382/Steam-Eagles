using System;
using System.Collections;
using Buildables;
using Buildings;
using Codice.Client.BaseCommands;
using Codice.CM.Client.Differences.Graphic;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;
using Zenject;

namespace Damage.Traps
{
    public class TripwireTrap : TriggeredTrap
    {
        public TripwireVisuals visuals;
        [Serializable]
        public class TripwireVisuals
        {
            public GameObject warningLight;
            public LineRenderer lineRenderer;
            public Gradient color;
            public Gradient detectColor;
            public Gradient triggeredColor;


            
            public void SetDetecting()
            {
                warningLight.SetActive(true);
                lineRenderer.colorGradient = detectColor;
            }
            public void SetTriggered()
            {
                warningLight.SetActive(true);
                lineRenderer.colorGradient = triggeredColor;
            }
            public void SetIdle()
            {
                warningLight.SetActive(true);
                lineRenderer.colorGradient = color;
            }
        }
        
        
        public LayerMask blockingMask;
        public LayerMask triggerMask;
        public int consecutiveHitsToTrigger = 5;
        public float checkInterval = 0.1f;
        
        private Vector3 _origin;
        private Vector2 _direction;
        private Vector3 _endPoint;
        private bool _hasEndPoint = false;
        
        private float _timer = 0f;

        private bool _isTriggered
        {
            get => isTriggered.Value;
            set => isTriggered.Value = value;
        }
        private bool _tilemapsDirty = false;

        public BoolReactiveProperty isDetecting = new();
        public BoolReactiveProperty isTriggered = new();

        protected override void Setup(Building building)
        {
            base.Setup(building);
            StartCoroutine(CheckForTriggered());
        }


        IEnumerator CheckForTriggered()
        {
            _timer = 0;
            int hitCount = 0;
            while (!_isTriggered)
            {
                yield return new WaitForSeconds(checkInterval);
                if (!CheckCast())
                {
                    hitCount = 0;
                    isDetecting.Value = false;
                    visuals.SetIdle();
                }
                else
                {
                    isDetecting.Value = true;
                    hitCount++;
                    if (hitCount >= consecutiveHitsToTrigger)
                    {
                        _isTriggered = true;
                    }
                    visuals.SetDetecting();
                }
            }
            visuals.SetTriggered();
        }


        private bool CheckCast()
        {
            if (_tilemapsDirty)
            {
                _tilemapsDirty = false;
                var blockingHit = Physics2D.Raycast(_origin, _direction, 10000, blockingMask);
                _hasEndPoint = blockingHit;
                if(_hasEndPoint) _endPoint = blockingHit.point;
            }
           
            if (_hasEndPoint)
            {
                return Physics2D.Linecast(_origin, _direction, triggerMask);
            }
            else
            {
               return Physics2D.Raycast(_origin, _direction, 10000, triggerMask);
            }
        }

        protected override void OnTrapFullySetup(Vector2 direction, BuildingCell cell)
        {
            _origin = Building.Map.CellToWorldCentered(cell.cell, cell.layers);
            _direction = direction;
            var hit = Physics2D.Raycast(_origin, _direction, 10000, blockingMask);
            _hasEndPoint = hit;
            if (_hasEndPoint) _endPoint = hit.point;
        }

        public override bool IsTriggered()
        {
            return _isTriggered;
        }

        public override void OnTriggered()
        {
            
        }
    }

    
    [Serializable]
    public class TrapSaveData
    {
        public Vector3Int cell;
        public Vector2 facingDirection;
    }

    public abstract class TriggeredTrap : TrapBase
    {
        [FormerlySerializedAs("triggerEffect")] public GameFX triggerFX;
        public TrapTriggerEffect triggerEffect;

        public float triggerDelay = 0.5f;
        public TriggerBehaviour triggerBehaviour = TriggerBehaviour.DESTROY;
        public enum TriggerBehaviour
        {
            DESTROY,
            DISABLE,
            NONE
        }
        private bool _hasBeenTriggered = false;
        
        public abstract bool IsTriggered();
        public abstract void OnTriggered();


        private void Update()
        {
            if (_hasBeenTriggered) return;
            if (IsTriggered())
            {
                _hasBeenTriggered = true;
                StartCoroutine(DoTriggered());
            }
        }

        private void TriggerTrap()
        {
            var triggerPoint = new GameObject(name + " Trigger Point").transform;
            triggerPoint.parent = transform.parent;
            triggerPoint.position = transform.position;
            var rot = triggerPoint.rotation.eulerAngles.z;
            triggerPoint.rotation = this.transform.rotation;
            if(triggerFX != null)triggerFX.SpawnEffectFrom(triggerPoint);
            switch (triggerBehaviour)
            {
                case TriggerBehaviour.DESTROY:
                    GameObject.Destroy(gameObject);
                    break;
                case TriggerBehaviour.DISABLE:
                    gameObject.SetActive(false);
                    break;
                case TriggerBehaviour.NONE:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            OnTriggered();
        }

        IEnumerator DoTriggered()
        {
            for (float t = 0; t < triggerDelay; t += Time .deltaTime)
            {
                DisplayTriggeredWarning(triggerDelay - t, t / triggerDelay);
                yield return null;
            }
            TriggerTrap();
            
        }

        protected virtual void DisplayTriggeredWarning(float timeUntilTriggered, float normalizedTimeUntilTriggered)
        {
            
        }
        
    }
    
    public abstract class TrapBase : MonoBehaviour, IMachineCustomSaveData
    {
        private Building _building;
        private Vector2 _facingDirection;



        public PlacementConfig placementConfig;
        
        public Transform trapOrigin => placementConfig.trapOrigin;
        
        [Serializable]
        public class PlacementConfig
        {
            [ChildGameObjectsOnly] public Transform trapOrigin;
            public float placementOffset = 0.1f;
        }
        private BuildingCell _cell;

        public Building Building => _building == null ? _building = GetComponentInParent<Building>() : _building;


        public Vector2 FacingDirection
        {
            get => _facingDirection;
            set
            {
                _facingDirection = value;
                SetFacingDirection(value);
            }
        }

        
        

        bool TryGetBuildingCell(out BuildingCell cell)
        {
            if (Building == null)
            {
                cell = default;
                return false;
            }

            var position = transform.position;
            var pos = position;
            var gridPos = Building.Map.WorldToCell(position, BuildingLayers.SOLID);
            
            cell = new BuildingCell(gridPos, GetTargetLayer());
            return true;
        }

        public virtual BuildingLayers GetTargetLayer() => BuildingLayers.SOLID;
        
        public void SetBuilding(Building building)
        {
            _building = building;
           
        }

       private IEnumerator Start()
       {
           while (_building == null)
               yield return null;
           Setup(_building);
       }

       protected virtual void Setup(Building building)
       {
           _building = building;
           var res = TryGetBuildingCell(out var cell);
           Debug.Assert(res, this);
           Setup(Building);
           if (res)
           {
               SetCellPosition(Building.Map.CellToWorldCentered(cell.cell, cell.layers), cell);
           }
       }


       private void SetCellPosition(Vector3 cellToWorldCentered, BuildingCell cell)
       {
           this._cell = cell;
           transform.position = cellToWorldCentered;
            
           var upCell = Building.Map.GetTile(cell.cell + Vector3Int.up, GetTargetLayer());
           var downCell = Building.Map.GetTile(cell.cell + Vector3Int.down, GetTargetLayer());
           var leftCell = Building.Map.GetTile(cell.cell + Vector3Int.left, GetTargetLayer());
           var rightCell = Building.Map.GetTile(cell.cell + Vector3Int.right, GetTargetLayer());
            
           var wallUp = upCell != null;
           var wallDown = downCell != null;
           var wallLeft = leftCell != null;
           var wallRight = rightCell != null;
            
           if(!wallDown && !wallUp && !wallLeft && !wallRight)
           {
               if(FacingDirection == Vector2Int.zero)
                   FacingDirection = Vector2Int.up;
               return;
           }

           if (wallRight ^ wallLeft)
           {
               FacingDirection = wallRight ? Vector2.left : Vector2.right;
           }
           else if (wallUp ^ wallDown)
           {
               FacingDirection = wallUp ? Vector2.down : Vector2.up;
           }
           else
           {
               FacingDirection = Vector2.up;
           }
           SetFacingDirection(FacingDirection);
           OnTrapFullySetup(FacingDirection, cell);
       }

       protected virtual void SetFacingDirection(Vector2 direction)
       {
           if (direction == Vector2.zero) return;
            FacingDirection = direction;
            float angle = Vector2.SignedAngle(Vector2.right, direction);
            transform.rotation = Quaternion.Euler(0, 0, angle);
            var cellSize = Building.Map.GetCellSize(GetTargetLayer());
            var extent = cellSize / 2f;
            var offset = direction * (extent - Vector2.one * placementConfig.placementOffset);
            var center = Building.Map.CellToWorldCentered(_cell.cell, _cell.layers);
            var start = (Vector2)center + offset;
            trapOrigin.position = start;
       }


       protected virtual void OnTrapFullySetup(Vector2 direction, BuildingCell cell)
       {
           
       }
       
       
       public void LoadDataFromJson(string json)
       {
           TrapSaveData data = JsonUtility.FromJson<TrapSaveData>(json);    
           this.FacingDirection = data.facingDirection;
           _cell = new BuildingCell(data.cell, GetTargetLayer());
           OnTrapFullySetup(FacingDirection, _cell);
       }

       public string SaveDataToJson()
       {
           TrapSaveData data = new TrapSaveData();
            data.facingDirection = this.FacingDirection;
            data.cell = _cell.cell;
            return JsonUtility.ToJson(data);
       }
    }
}