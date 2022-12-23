using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings.BuildingTilemaps
{
    public abstract class RenderedTilemap : BuildingTilemap
    {
        private TilemapRenderer _tmr;
        public TilemapRenderer TilemapRenderer => _tmr ? _tmr : _tmr = GetComponent<TilemapRenderer>();

        public virtual string GetSortingLayerName(Building building)
        {
            return "Default";
        } 
        
        public abstract int GetSortingOrder(Building building);

        public override void UpdateTilemap(Building building)
        {
            TilemapRenderer.sortingOrder = GetSortingOrder(building);
            TilemapRenderer.sortingLayerID = SortingLayer.NameToID(GetSortingLayerName(building));
        }
    }
}