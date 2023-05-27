using System;
using System.Collections.Generic;
using Buildings.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Buildings
{
    [Serializable]
    public class TileAssets
    {
        [LabelWidth(75)]  [Required] public SolidTile solidTile;
        [LabelWidth(75)]  public WallTilePair wallTiles;
        [LabelWidth(75)]  public PipeTilePair pipeTiles;


        
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