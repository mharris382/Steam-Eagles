using Buildings;
using UnityEngine;

namespace UI.Crafting.Destruction
{
    public class DestructionPreview
    {
        public GameObject destructionPreview;
        private SpriteRenderer _spriteRenderer;

        public Color color
        {
            set
            {
                if(_spriteRenderer == null)RecreatePreview();
                _spriteRenderer.color = value;
            }
        }

        public void UpdateTarget(Building building, BuildingCell buildingCell, Vector2 size)
        {
            var position = buildingCell.cell2D;
            var min = buildingCell.cell2D;
            _spriteRenderer.size = size;
            _spriteRenderer.transform.parent = building.transform;
            _spriteRenderer.transform.position = building.Map.CellToWorld(buildingCell);
        }
        
        public DestructionPreview()
        {
            RecreatePreview();
        }

        private void RecreatePreview()
        {
            var go = new GameObject("Destruction Preview");
            destructionPreview = go;
            _spriteRenderer = destructionPreview.AddComponent<SpriteRenderer>();
            _spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            _spriteRenderer.color = Color.clear;
            _spriteRenderer.sortingLayerName = "UI";
            _spriteRenderer.sprite = CraftingPreviewResources.Instance.destructionSprite;
        }
    }
}