using System;
using Buildings;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _EXP.PhysicsFun.ComputeFluid
{
    public class DynamicIOObject : MonoBehaviour
    {
        private static Color sinkColor = new Color(0.8f, 0.1f, 0.2f);
        private static Color sourceColor = new Color(0.1f, 0.8f, 0.2f);

        [SerializeField, Range(-1, 1), GUIColor(nameof(guiColor))]
        private float value = 0.1f;
        
        [OnValueChanged(nameof(ClampSize))]
        public Vector2Int size = new Vector2Int(1, 1);

        
        void ClampSize()
        {
            size.x = Mathf.Max(1, size.x);
            size.y = Mathf.Max(1, size.y);
        }
        
        private Color guiColor => value == 0 ? Color.gray : value > 0 ? sourceColor : sinkColor;
        
        
        private Building _building;
        public Building Building => _building ? _building : _building = GetComponentInParent<Building>();

        private GasTexture _gasTexture;
        public GasTexture GasTexture => _gasTexture ? _gasTexture : _gasTexture = GetComponentInParent<GasTexture>();


        bool HasResources => GasTexture != null && Building != null;
        private Vector2Int GetGridPosition()
        {
            if(Building == null) return Vector2Int.zero;
            var cell = Building.Map.WorldToCell(transform.position, BuildingLayers.SOLID);
            return new Vector2Int(cell.x * GasTexture.Resolution, cell.y * GasTexture.Resolution);
        }

        public virtual float GetTargetGasIOValue() => value;

        public virtual void OnGasIO(float gasDelta)
        {
            
        }

        private void OnDrawGizmosSelected()
        {
            if (!HasResources) return;
            var gridPos = GetGridPosition();
            var gridSize = new Vector2Int(GasTexture.Resolution, GasTexture.Resolution);
        }
    }
}