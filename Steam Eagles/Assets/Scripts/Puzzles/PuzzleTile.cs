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
    }
}