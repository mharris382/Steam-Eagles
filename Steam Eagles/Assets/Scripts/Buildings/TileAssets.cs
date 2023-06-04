using System;
using System.Collections.Generic;
using Buildings.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings
{
    [Serializable]
    public class TileAssets
    {
        [LabelWidth(75)]  [Required] public LadderTile ladderTile;
        [LabelWidth(75)]  [Required] public WireTile wireTile;
        [LabelWidth(75)]  [Required] public SolidTile solidTile;
        [LabelWidth(75)]  public WallTilePair wallTiles;
        [LabelWidth(75)]  public PipeTilePair pipeTiles;


        public TileBase GetDefaultTile(BuildingLayers layer)
        {
            switch (layer)
            {
                case BuildingLayers.NONE:
                    break;
                case BuildingLayers.WALL:
                    return wallTiles.repairedTile;
                    break;
                case BuildingLayers.FOUNDATION:
                case BuildingLayers.SOLID:
                    return solidTile;
                    break;
                case BuildingLayers.PIPE:
                    return pipeTiles.repairedTile;
                    break;
                case BuildingLayers.COVER:
                    break;
                case BuildingLayers.PLATFORM:
                    break;
                case BuildingLayers.DECOR:
                    break;
                case BuildingLayers.WIRES:
                    return wireTile;
                    break;
                case BuildingLayers.LADDERS:
                    return ladderTile;
                    break;
                case BuildingLayers.REQUIRED:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layer), layer, null);
            }
            throw new NotImplementedException();
        }
    }

    

    [Serializable]
    public class TileWithVariants<T> where T : EditableTile
    {
        [Required] public T tileDefault;

        [ShowIf("@tileDefault!=null")]
        [ListDrawerSettings(Expanded = true)]
        [SerializeField] private List<Variant> tileVariants;
        
        [Serializable] public class Variant
        {
            public string variantName;
            public T variant;
        }    
    }

    public class SolidTileVariants : TileWithVariants<SolidTile> { }


    /// <summary>
    /// editor helper which groups two tiles together 
    /// </summary>
    /// <typeparam name="TDamaged"></typeparam>
    /// <typeparam name="TRepaired"></typeparam>
    [Serializable, InlineProperty]
    public class TilePair<TDamaged, TRepaired> 
        where TDamaged : RepairableTile 
        where TRepaired : DamageableTile
    {
        [Required, HorizontalGroup(LabelWidth = 75), LabelText("Damaged"), ValidateInput(nameof(ValidateDamaged))] public TDamaged damagedTile;
        [Required, HorizontalGroup(LabelWidth = 75), LabelText("Repaired"), ValidateInput(nameof(ValidateRepair))] public TRepaired repairedTile;

        bool ValidateRepair(TRepaired repairableTile, ref string msg)
        {
            if (repairableTile == null || damagedTile == null)
            {
                msg = "Must assign both tiles";
                return false;
            }
            bool res = repairableTile.GetDamagedTileVersion() == damagedTile;
            msg = "Damaged tile must be the same as the repaired tile";
            return res;
        }

        bool ValidateDamaged(TDamaged damaged, ref string msg)
        {
            if (damaged == null || damagedTile == null)
            {
                msg = "Must assign both tiles";
                return false;
            }
            msg = "Damaged tile must be the same as the repaired tile";
            return damaged.GetRepairedTileVersion() == repairedTile;
        }
    }

    [Serializable] public class WallTilePair : TilePair<DamagedWallTile, WallTile> { }
    [Serializable] public class PipeTilePair : TilePair<DamagedPipeTile, PipeTile> { }
}