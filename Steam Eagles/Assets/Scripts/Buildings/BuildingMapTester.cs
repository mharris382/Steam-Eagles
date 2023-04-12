using System;
using Buildings.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Buildings
{
    public class BuildingMapTester : MonoBehaviour
    {
        [Required]
        public Building building;
        [Required]
        public EditableTile tile;
        [Required]
        public SpriteRenderer visual;

        public Color validColor = new Color(0.5f, 0.9f, 0.6f, 0.5f);
        public Color invalidColor = new Color(0.9f, 0.5f, 0.6f, 0.5f);
        
        private BuildingMap _map;

        private void Awake()
        {
            _map = building.Map;
            visual.drawMode = SpriteDrawMode.Sliced;
        }
        
        bool HasRequirements() => _map != null && tile != null && visual != null;

        private void Update()
        {
            if(!HasRequirements())return;
            
            var size = _map.GetCellSize(tile.GetLayer());
            visual.size = size;
            
            var worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPoint.z = 0;
            var cellPos = _map.WorldToCell(worldPoint, tile.GetLayer());
            var visPos = _map.CellToWorld(cellPos, tile.GetLayer());
            visual.transform.position = visPos;
            bool valid = tile.IsPlacementValid(cellPos, _map);
            visual.color = valid ? validColor : invalidColor;
        }
    }
}