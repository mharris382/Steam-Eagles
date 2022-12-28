using UnityEngine;
using UnityEngine.Tilemaps;

namespace Spaces
{
    public abstract class PuzzleTile : RuleTile
    {
        public bool debug;
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
    }

    public abstract class EditableTile : PuzzleTile
    {
        public DynamicBlock dynamicBlock;
        public override bool CanTileBeDisconnected()
        {
            Debug.Assert(dynamicBlock!=null, $"Tile {name} is missing a DynamicBlock!", this);
            return true;
        }
    }
}