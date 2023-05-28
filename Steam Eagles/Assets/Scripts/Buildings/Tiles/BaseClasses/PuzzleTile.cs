using System.Collections.Generic;
using Buildings.Tiles.Skin;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings.Tiles
{
    public interface IPuzzleTile : IVariant
    {
        string name { get; }
    }
    public abstract class PuzzleTile : RuleTile, IPuzzleTile
    {
        public string VariantKey => !isVariant ? name : $"{tileRoot}_{name}";
        

        [ToggleGroup(nameof(isVariant))] public bool isVariant;
        [ToggleGroup(nameof(isVariant))] public string tileRoot;
        
       
        
        public bool debug;
        public bool alwaysMatchNeighbors = true;

        [ShowIf(nameof(alwaysMatchNeighbors))]
        public List<PuzzleTile> matchNeighbors;

        public abstract bool CanTileBeDisconnected();


        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject instantiatedGameObject)
        {
            if(debug)
                Debug.Log($"StartUp {this.name} - {position}", this);
            return base.StartUp(position, tilemap, instantiatedGameObject);
        }

        public override bool RuleMatches(TilingRule rule, Vector3Int position, ITilemap tilemap, ref Matrix4x4 transform)
        {
            if (RuleMatches(rule, position, tilemap, 0))
            {
                transform = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 0f), Vector3.one);
                return true;
            }

            return false;
        }

        public override bool RuleMatch(int neighbor, TileBase other)
        {
            if (!(other is PuzzleTile tile))
            {
                return base.RuleMatch(neighbor, other);
            }

            if (!alwaysMatchNeighbors) return base.RuleMatch(neighbor, tile);
            switch (neighbor)
            {
                case TilingRuleOutput.Neighbor.This:
                {
                    return TileIsMatch(other);
                }
                case TilingRuleOutput.Neighbor.NotThis:
                {
                    return !TileIsMatch(other);
                }
            }
            return base.RuleMatch(neighbor, tile);
        }

        protected virtual bool TileIsMatch(TileBase other)
        {
            var src = this;
            
            if (other == null)
                return false;
            if (other == src)
                return true;
            foreach (var matchNeighbor in GetMatchingTiles())
            {
                if (matchNeighbor == other) return true;
            }
            return false;
        }

        protected virtual IEnumerable<TileBase> GetMatchingTiles() => matchNeighbors;
    }
}