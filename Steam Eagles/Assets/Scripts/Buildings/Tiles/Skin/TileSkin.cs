using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings.Tiles.Skin
{
    [CreateAssetMenu(fileName = "New Tile Skin", menuName = "Steam Eagles/Tiles/New Tile Skin", order = 0)]
    public class TileSkin : SerializedScriptableObject
    {
        
        [ValidateInput(nameof(ValidateTiles)),HideLabel,TabGroup("Solids")]public PuzzleTileVariant<SolidTile> solidTiles;
        [ValidateInput(nameof(ValidateTiles)),HideLabel,TabGroup("Pipes")]public PuzzleTileVariant<EditableTile> pipeTiles;
        [ValidateInput(nameof(ValidateTiles)),HideLabel,TabGroup("Wires")]public PuzzleTileVariant<WireTile> wireTiles;
        [ValidateInput(nameof(ValidateTiles)),HideLabel,TabGroup("Ladders")]public PuzzleTileVariant<LadderTile> ladderTiles;
        [ValidateInput(nameof(ValidateTiles)),HideLabel,TabGroup("Walls")]public PuzzleTileVariant<EditableTile> wallTiles;
        
        
        private IEnumerable<PuzzleTileVariant> Variants
        {
            get
            {
                yield return solidTiles;
                yield return pipeTiles;
                yield return wireTiles;
                yield return ladderTiles;
                yield return wallTiles;
            }
        }


        private bool ValidateTiles()
        {
            foreach (var puzzleTileVariant in Variants)
            {
                if (puzzleTileVariant.Variants.Count == 0)
                {
                    Debug.LogError($"Tile Skin {name} has no variants for {puzzleTileVariant.GetType().Name}");
                    return false;
                }
            }

            return true;
        }
    }



    public interface IVariant
    {
        string VariantKey { get; }
    }

    [InlineProperty(LabelWidth = 150)]
    [Serializable]
    public class PuzzleTileVariant<T> : PuzzleTileVariant where T : PuzzleTile { }
    public class PuzzleTileVariant : Variant<IPuzzleTile> { }
    [Serializable]
    public class Variant<T> where T : IVariant
    {
        [OdinSerialize, HideInInspector] private List<T> variants = new List<T>();

        [ShowInInspector, ListDrawerSettings(Expanded = true)]
        internal List<T> Variants
        {
            get => variants ??= new List<T>();
            set => variants = value;
        }
        private T Root
        {
            get
            {
                if (variants == null || variants.Count == 0)
                {
                    variants = new List<T>();
                }
                var v = variants[0];
                
                return v;
            }
            set
            {
                if(value == null) return;
                if (variants.Contains(value))
                {
                    var tmp = variants[0];
                    variants[0] = value;
                    variants[variants.IndexOf(value)] = tmp;
                }
            }
        }

        public T this[string variantKey]
        {
            get
            {
                if (string.IsNullOrEmpty(variantKey))
                    return Root;
                return variants.Find(v => v.VariantKey == variantKey);
            }
        }

        public string MainKey => Root.VariantKey;
    }
}