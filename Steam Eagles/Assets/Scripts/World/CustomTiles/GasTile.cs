using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using World.GasSim;

namespace World.CustomTiles
{
    [CreateAssetMenu(fileName = "new gas tile", menuName = "2D/Tiles/New Gas Tile", order = 0)]
    public class GasTile : Tile
    {
        public SimulationState simulationState;
        [SerializeField] SimulationConfig simulationConfig;

        [Range(1, 16)]
        public int tilePressure = 1;


        private Color _color;
        private void OnEnable()
        {
            float t = tilePressure / (float)simulationConfig.maxGasDensity;
            Color c = simulationConfig.gasColorGradient.Evaluate(t);
            _color = c;
        }

        public override void RefreshTile(Vector3Int position, ITilemap tilemap)
        {
                
            base.RefreshTile(position, tilemap);
        }
        

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            if (!simulationState.IsPressuredChanged(position, tilePressure))
            {
                return;
            }

            base.GetTileData(position, tilemap, ref tileData);
            tileData.color = _color;
        }
    }
}