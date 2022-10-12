using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using World.CustomTiles;
using Random = UnityEngine.Random;

namespace World.GasSim
{
    public class SimulationController : MonoBehaviour
    {

        public SimulationState simulationState;
        public SharedTilemap gasTilemap;
        public SharedTilemap blockTilemap;
        public SharedTiles gasTileLookup;
        
        public ComputeShader simulationComputeShader;
        public SharedRenderTexture solidRenderTexture;
        public SharedRenderTexture velocityFieldTexture;
        

        public bool enableComputeShader = true;
        
        private Dictionary<Vector3Int, int> gasCells = new Dictionary<Vector3Int, int>();
        private Dictionary<Vector3Int, int> nextCells = new Dictionary<Vector3Int, int>();

        private GasCell[] data;
        
        public struct GasCell
        {
            public Vector3Int position;
            public int density;
            //public Vector2 velocity; velocity is stored in the tilemap because it is really only needed by the compute shader.
            //however it must be computed on two passes, once to solve the velocity, once to apply it

            public GasCell(Vector3Int position, int density)
            {
                this.position = position;
                this.density = density;
            }
        }

        private void Awake()
        {
            simulationState.gasAddedToTilemap.AddListener(OnGasAddedToTilemap);
        }


        private void Start()
        {
            StartCoroutine(RunSimulation());
        }

        void OnGasAddedToTilemap(Vector3Int position, int density)
        {
            if (gasCells.ContainsKey(position))
            {
                gasCells[position] = density;
            }
            else
            {
                gasCells.Add(position, density);
            }
            //iteratively search neighbors to create buffer 
            if (density > 0) CheckNeighbors(position);
        }

        void CheckNeighbors(Vector3Int position)
        {
            Vector3Int[] neighbors = new Vector3Int[]
            {
                position + Vector3Int.right,
                position + Vector3Int.left,
                position + Vector3Int.down,
                position + Vector3Int.up
            };
            foreach (var neighbor in neighbors)
            {
                if (gasTilemap.Value.GetTile(neighbor) == null)
                {
                    OnGasAddedToTilemap(neighbor, 0);
                }
            }
        }

        private IEnumerator RunSimulation()
        {
            simulationState.IsRunning = true;
            while (simulationState.IsRunning)
            {
                if (simulationState.Stage == SimulationStage.IDLE)
                {
                    if (gasCells.Count > 0) simulationState.Stage = SimulationStage.UPDATE_PRESSURE;
                    
                }

                yield return new WaitForSeconds(simulationState.Rate);
                
                simulationState.Stage = SimulationStage.UPDATE_PRESSURE;
                gasTilemap.Value.RefreshAllTiles();
                UpdatePressure();
                
                if (simulationState.Stage == SimulationStage.IDLE) continue;
                
                simulationState.Stage = SimulationStage.CALCULATE_VELOCITY;
                ExecuteCalculateVelocity();
                
                simulationState.Stage = SimulationStage.APPLY_PRESSURE;
                ApplyVelocityPressure();
                yield return null;
            }
        }

        private void UpdatePressure()
        {
            if (gasCells.Count == 0)
            {
                
                simulationState.Stage = SimulationStage.IDLE;
                return;
            }

            data = gasCells.Select(t => new GasCell(t.Key, t.Value)).ToArray();
        }

        float Rand(Vector2 co) => (Mathf.Sin(Vector2.Dot(co, new Vector2(12.9898f, 78.233f))) * 43758.5453f) % 1;


        private ComputeBuffer _computeBuffer;
        void ExecuteCalculateVelocity()
        {
            if (enableComputeShader)
            {
                int v3size = sizeof(int) * 3;
                int isize = sizeof(int);
                _computeBuffer = new ComputeBuffer(data.Length, v3size + isize);
                _computeBuffer.SetData(data);
                
                
                simulationComputeShader.SetBuffer(0, "cells", _computeBuffer);
                simulationComputeShader.SetTexture(0, "solid", solidRenderTexture.Value);
                simulationComputeShader.SetTexture(0, "velocity", velocityFieldTexture.Value);
                simulationComputeShader.SetFloat("time", Time.time);
                simulationComputeShader.SetFloat("random", Random.value);
                simulationComputeShader.Dispatch(0, data.Length / 10, 1, 1);
                _computeBuffer.GetData(data);
                _computeBuffer.Dispose();
            }
            else
            {
                ExecuteCalaculteVelocityCPU();
            }
        }
    
        private void ExecuteCalaculteVelocityCPU()
        {
            
            bool GetValue(int pu, Dictionary<Vector3Int, HashSet<Vector3Int>> tranferRecord, Vector3Int u,
                KeyValuePair<Vector3Int, int> gasCell, ref int p)
            {
                if (u.y < 10) return false;
                if (blockTilemap.Value.HasTile(u)) return false;
                if (pu < p)
                {
                    if (tranferRecord.ContainsKey(u))
                    {
                        if (tranferRecord[u].Contains(gasCell.Key))
                        {
                            return false;
                        }
                    }

                    if (AttemptTransfer(ref p, ref pu, 1))
                    {
                        //transfer successfully
                        UpdateGas(gasCell.Key, p);
                        UpdateGas(u, pu);
                        return true;
                    }
                }

                return false;
            }
            
            nextCells = new Dictionary<Vector3Int, int>();

            Vector3Int?[] neighbors = new Vector3Int?[4];
            Dictionary<Vector3Int, HashSet<Vector3Int>> tranferRecord = new Dictionary<Vector3Int, HashSet<Vector3Int>>();
            foreach (var gasCell in gasCells)
            {
                int p = GetGas(gasCell.Key);
                tranferRecord.Add(gasCell.Key, new HashSet<Vector3Int>());
                var pos = gasCell.Key;
                var u = pos + Vector3Int.up;
                var r = pos + Vector3Int.right;
                var l = pos + Vector3Int.left;
                var d = pos + Vector3Int.down;

                p = GetGas(u);
                if (GetValue(GetGas(u), tranferRecord, u, gasCell, ref p))
                {
                    if (Random.value > 0.05f)
                    {
                        continue;
                    }
                }

                if (GetValue(GetGas(r), tranferRecord, r, gasCell, ref p))
                {
                    if (Random.value > 0.1f)
                    {
                        continue;
                    }
                }

                if (GetValue(GetGas(l), tranferRecord, l, gasCell, ref p))
                {
                    if (Random.value > 0.2f)
                    {
                        continue;
                    }
                }

                if (GetValue(GetGas(d), tranferRecord, d, gasCell, ref p)) continue;
            }

            data = nextCells.Select(t => new GasCell(t.Key, t.Value)).ToArray();

            gasCells = nextCells;
        }

        private void UpdateGas(Vector3Int p0, int pu)
        {
            if (!nextCells.TryAdd(p0, pu)) nextCells[p0] = pu;
        }

        bool AttemptTransfer(ref int fromAmount, ref int toAmount, int transferAmount)
        {
            transferAmount = Mathf.Max(fromAmount, transferAmount);
            if (toAmount + transferAmount > 16)
            {
                return false;
            }

            fromAmount -= transferAmount;
            toAmount += transferAmount;
            return true;
        }
        
        int GetGas(Vector3Int pos)
        {
            if (nextCells.ContainsKey(pos)) return nextCells[pos];
            var tile = gasTilemap.Value.GetTile<GasTile>(pos);
            return tile == null ? 0 : tile.tilePressure;
        }

        bool IsValid(Vector3Int pos)
        {
            return gasCells.ContainsKey(pos);
        }

        private void ApplyVelocityPressure()
        {
            foreach (var gasCell in data)
            {
                var pos = gasCell.position;
                var tile = gasCell.density == 0 ? null : gasTileLookup.Value[gasCell.density - 1];
                gasTilemap.Value.SetTile(pos, tile);
            }
        }
    }
}