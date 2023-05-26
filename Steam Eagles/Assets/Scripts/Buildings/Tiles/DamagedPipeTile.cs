using Buildings.Rooms;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings.Tiles
{
    public class DamagedPipeTile : RepairableTile
    {
        [Required, SerializeField,ValidateInput(nameof(Validate))]
        private PipeTile repairedTileVariant;

        #region [Editor Only]

        bool Validate(PipeTile repaired, ref string msg)
        {
            if (repaired == null)
            {
                return true;
            }
            msg = "Damaged tile and pipe tile must reference each other";
            return repairedTileVariant.GetDamagedTileVersion() == this;
        }

        #endregion

        public override bool CanTileBePlacedInRoom(Room room) => repairedTileVariant.CanTileBePlacedInRoom(room);

        public override BuildingLayers GetLayer() => repairedTileVariant.GetLayer();

        public override bool IsPlacementValid(Vector3Int cell, BuildingMap buildingMap) => repairedTileVariant.IsPlacementValid(cell, buildingMap);

        public override DamageableTile GetRepairedTileVersion() => repairedTileVariant;

        protected override bool TileIsMatch(TileBase other)
        {
            if (other == repairedTileVariant)
                return true;
            return base.TileIsMatch(other);
        }
    }
}