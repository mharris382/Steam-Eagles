using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace World
{
    [CreateAssetMenu(menuName = "Shared Variables/Dynamic Tilemap", fileName = "New Dynamic Tilemap", order = 0)]
    public class DynamicTilemap : SharedTilemap
    {
        
        public void RemoveTile(Vector3Int position)
        {
            if (!CheckValid(nameof(HasTile))) return;
            throw new NotImplementedException();
        }

        public void PlaceTile(Vector3Int position, TileBase tile)
        {
            if (!CheckValid(nameof(HasTile))) return;
            throw new NotImplementedException();
        }
        public T GetTile<T>(Vector3Int position) where T : TileBase
        {
            if (!CheckValid(nameof(HasTile))) return null;
            throw new NotImplementedException();
        }

        public bool HasTile(Vector3Int position)
        {
            if (!CheckValid(nameof(HasTile))) return false;
            throw new NotImplementedException();
        }

        bool CheckValid(string callingMethod)
        {
            if (!HasValue)
            {
                Debug.Log($"Calling {callingMethod} on invalid dynamic tilemap", this);
                return false;
            }
            return true;
        }
    }
}