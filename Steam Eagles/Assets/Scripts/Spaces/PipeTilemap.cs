using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using World;

namespace Spaces
{
    [RequireComponent(typeof(Tilemap))]
    public class PipeTilemap : MonoBehaviour
    {
        public PipeTile pipeTile;

        private Tilemap _tilemap;
        
        private void Awake()
        {
            _tilemap = GetComponent<Tilemap>();
        }
    }


    public abstract class PuzzleTile : RuleTile
    {
        public abstract bool CanTileBeDisconnected();


    }
}