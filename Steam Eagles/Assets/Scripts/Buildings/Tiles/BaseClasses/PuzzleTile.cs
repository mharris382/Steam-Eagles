using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings.Tiles
{
    public abstract class PuzzleTile : RuleTile
    {
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

        private bool TileIsMatch(TileBase other)
        {
            if (other == null)
                return false;
            if (other == this)
                return true;
            foreach (var matchNeighbor in matchNeighbors)
            {
                if (matchNeighbor == other)
                    return true;
            }

            return false;
        }
    }
}