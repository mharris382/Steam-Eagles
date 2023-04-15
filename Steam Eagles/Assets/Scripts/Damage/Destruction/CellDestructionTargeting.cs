using System;
using Buildings;
using CoreLib;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Damage.Destruction
{
    public class CellDestructionTargeting : SerializedMonoBehaviour
    {
        [SerializeField] private Building targetBuilding;


        private BuildingLayers targetLayers = BuildingLayers.SOLID;

        [SerializeField] private Vector2Int[] cells;


        private void OnDrawGizmos()
        {
            if (targetBuilding == null)
            {
                targetBuilding = FindObjectOfType<Building>();
            }

            if (targetBuilding == null) return;
            
            var cell = targetBuilding.Map.WorldToCell(transform.position, targetLayers);
            foreach (var vector2Int in cells)
            {
                Gizmos.color = Color.red.SetAlpha(0.5f);
                var worldPos = targetBuilding.Map.CellToWorld(cell + (Vector3Int)vector2Int, targetLayers);
                Gizmos.DrawWireCube(worldPos, targetBuilding.Map.GetCellSize(targetLayers));
            }
        }
    }
}