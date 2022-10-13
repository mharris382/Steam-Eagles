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

        public SharedTilemap gasTilemap;
        
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

        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
        {
            if (go == null) return false;
            var tm = go.GetComponent<Tilemap>();
            Debug.Assert(tm != null);
            if(gasTilemap.Value == null) gasTilemap.Value = tm;
            if (gasTilemap.Value == tm)
            {
                simulationState.RegisterGasToSim(position, tilePressure);
                return true;
            }
            return false;
        }
        
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            if (simulationState.IsRunning)
            {
                if (simulationState.Stage == SimulationStage.IDLE ||
                    simulationState.Stage == SimulationStage.UPDATE_PRESSURE || 
                    simulationState.Stage == SimulationStage.UPDATE_PRESSURE_TILEMAP)
                {
                    simulationState.RegisterGasToSim(position, tilePressure);
                }
            }
            base.GetTileData(position, tilemap, ref tileData);
            tileData.color = _color;
        }
    }
}