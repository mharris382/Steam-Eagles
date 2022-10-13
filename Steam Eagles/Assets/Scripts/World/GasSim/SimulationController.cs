using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using World.CustomTiles;
using Random = UnityEngine.Random;

namespace World.GasSim
{
    public class SimulationController : MonoBehaviour
    {
        [Tooltip("Shared Data container for simulation allowing communication between the simulation and external systems")]
        public SimulationState simulationState;
        
        [Header("Compute Shaders")] 
        public bool enableComputeShader = true;
        
        [FormerlySerializedAs("copyBufferToTexture")] 
        [Tooltip("handles the transfer of data from the CPU to the GPU right before the simulation is updated. Writes to the gas texture, reads from compute buffer")]
        public ComputeShader bufferToTexture;
        
        [Tooltip("Updates the velocity field texture based on the gas texture. writes to the velocity texture, reads from the gas texture")]
        public ComputeShader velocityPass;
        [Tooltip("updates the gas state based on the velocity texture. writes to the gas texture, reads from velocity texture")]
        public ComputeShader pressurePass;
        [Tooltip("handles the transfer of data from the GPU back to the CPU after the simulation has completed. Writes to the compute buffer, reads from gas texture")]
        public ComputeShader textureToBuffer;
        [System.Obsolete("use velocityPass instead")]
        public ComputeShader simulationComputeShader;
        
        
        [Header("Tilemaps")]
        public SharedTilemap blockTilemap;
        [Tooltip("needed to apply new density states after the simulation has updated")]
        public SharedTilemap gasTilemap;
        [Tooltip("used to get the gas tiles from density states to apply the new gas state back to the tilemap after the simulation has updated")]
        public SharedTiles gasTileLookup;
        
    
        
        [Header("Render Textures")]
        [Tooltip("shared reference to a render texture which will be used by the compute shaders to determine where the solid blocks are")]
        public SharedRenderTexture solidRenderTexture;
        
        [Tooltip("shared reference to velocity field texture which will be written to during the velocity pass and read from during the pressure pass")]
        public SharedRenderTexture velocityFieldTexture;
        
        [Tooltip("only used by the simulation compute shaders.  This is the texture that will be copied to/from the buffer")]
        public SharedRenderTexture gasTexture;
        
        
        private Dictionary<Vector3Int, int> gasCells = new Dictionary<Vector3Int, int>();
        private Dictionary<Vector3Int, int> nextCells = new Dictionary<Vector3Int, int>();

        private GasCell[] data;

        [Header("Simulation Bounds")]
        public Vector2Int simulationBoundsMin;
        public Vector2Int simulationBoundsMax = new Vector2Int(100, 100);

        [Header("Simulation Parameters")] public Vector3 globalWind = Vector3.right;
        
        private ComputeBuffer _computeBuffer;

        private bool IsSimulationValid
        {
            get
            {
                if (GasTexture == null || VelocityTexture == null || SolidTexture == null)
                {
                    Debug.LogError("missing one or more of textures needed to run compute shaders",this);
                    return false;
                }
                
                if (data == null || data.Length == 0)
                {
                    return false;
                }
                return true;
            }
        }

        private RenderTexture VelocityTexture
        {
            get
            {
                if (!velocityFieldTexture.HasValue)
                {
                    //TODO: initialize velocity field rt
                    throw new NotImplementedException("TODO: initialize velocity field rt");
                }
                return velocityFieldTexture.Value;
            }
        }
        private RenderTexture SolidTexture
        {
            get
            {
                if (!solidRenderTexture.HasValue)
                {
                    //TODO: initialize solid rt
                    throw new NotImplementedException("TODO: initialize solid rt");
                }
                return solidRenderTexture.Value;
            }
        }
        private RenderTexture GasTexture
        {
            get
            {
                if (!gasTexture.HasValue)
                {
                    var rt = new RenderTexture(velocityFieldTexture.Value.width, velocityFieldTexture.Value.height,
                        velocityFieldTexture.Value.depth);
                    rt.enableRandomWrite = true;
                    rt.Create();
                    gasTexture.Value = rt;
                }

                return gasTexture.Value;
            }
        }

        private Tilemap GasTilemap
        {
            get
            {
                return gasTilemap.Value;
            }
        }
        
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
            
        }


        private void Start()
        {
            StartCoroutine(RunSimulation());
        }


        private IEnumerator RunSimulation()
        {
            simulationState.IsRunning = true;
            while (true)
            {
                
                yield return null;//safety measure to prevent infinite loops from occuring
                
                if (!simulationState.IsRunning)
                    continue;

                if (simulationState.Stage == SimulationStage.IDLE && gasCells.Count > 0)
                    simulationState.Stage = SimulationStage.UPDATE_PRESSURE;

                yield return new WaitForSeconds(simulationState.Rate);
                
                simulationState.Stage = SimulationStage.UPDATE_PRESSURE_TILEMAP;
                
                
                gasTilemap.Value.RefreshAllTiles();//sync simulation state with the tilemap state
                
                CellDictionaryToCellData(); //sync simulation data(cell array) with simulation state(dictionary)
                
                if (data == null || data.Length == 0)
                    continue;//if simulation has no gas, go back to beginning of loop and check again on next loop
                
                yield return null;
                BufferToTexPass();
                yield return null;
                VelocityPass();
                yield return null;
                PressurePass();
                yield return null;
                TexToBufferPass();
                yield return null;
                CellDataToTilemapData();
            }
        }

        #region [GPU Passes]

        private void VelocityPass()
        {
            simulationState.Stage = SimulationStage.CALCULATE_VELOCITY;
            
            SetGasInComputeShader(velocityPass);
            SetVelocityInComputeShader(velocityPass);
            SetSolidInComputeShader(velocityPass);
            SetShaderVariables(velocityPass);
            
            DispatchVelocityPass();
        }

        private void PressurePass()
        {
            simulationState.Stage = SimulationStage.APPLY_PRESSURE;
            
            SetGasInComputeShader(pressurePass);
            SetVelocityInComputeShader(pressurePass);
            
            SetSolidInComputeShader(pressurePass);
            SetShaderVariables(pressurePass);
            
            DispatchPressurePass();
        }
        
        private void BufferToTexPass()
        {
            //needed for sure
            SetGasInComputeShader(bufferToTexture);
            SetBufferInComputeShader(bufferToTexture);
            
            //not sure if needed
            SetSolidInComputeShader(bufferToTexture);
            SetShaderVariables(textureToBuffer);
            
            DispatchBufferToTexture();
        }
        
        private void TexToBufferPass()
        {
            //needed for sure
            SetGasInComputeShader(textureToBuffer);
            SetBufferInComputeShader(textureToBuffer);
            
            //not sure if needed
            SetShaderVariables(textureToBuffer);
            SetSolidInComputeShader(textureToBuffer);
            
            DispatchTextureToBuffer();
        }

        #endregion

        #region [Communication Helpers]

        #region [CPU Communication helpers]

        private void CellDictionaryToCellData() => data = gasCells.Select(t => new GasCell(t.Key, t.Value)).ToArray();

        private void CellDataToTilemapData()
        {
            Vector3Int[] cellPositions = new Vector3Int[data.Length];
            TileBase[] cellTiles = new TileBase[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                var cell = data[i];
                cellPositions[i] = cell.position;
                cellTiles[i]= cell.density == 0 ? null : gasTileLookup.Value[cell.density - 1];
            }
            GasTilemap.SetTiles(cellPositions, cellTiles);
        }
        

        #endregion

        #region [CPU to GPU communication helpers]

        void SetVelocityInComputeShader(ComputeShader computeShader) => computeShader.SetTexture(0, "velocity", VelocityTexture);

        void SetSolidInComputeShader(ComputeShader computeShader) => computeShader.SetTexture(0, "solid", SolidTexture);

        void SetGasInComputeShader(ComputeShader computeShader) => computeShader.SetTexture(0, "gas", GasTexture);

        void SetBufferInComputeShader(ComputeShader computeShader)
        {
            if (_computeBuffer != null)
            {
                _computeBuffer.Dispose();
            }
            int size = data.Length;
            int stride = sizeof(int) * 4;
            var cb = new ComputeBuffer(size, stride);
            cb.SetData(data);
            _computeBuffer = cb;
        }

        void SetShaderVariables(ComputeShader computeShader)
        {
            computeShader.SetInts("bounds", simulationBoundsMin.x, simulationBoundsMin.y, simulationBoundsMax.x, simulationBoundsMax.y);
            computeShader.SetFloats("globalWind", globalWind.x, globalWind.y, globalWind.z);
        }

        #endregion
        

        #endregion

        #region [Dispatch Helpers]

        const int THREADS_TEXTURE_PASSES = 8;
        const int THREADS_BUFFER_PASSES = 10;
        

        /// <summary>
        /// dispatches the bufferToTexture compute shader then disposes of the compute buffe
        /// <see cref="BufferToTexPass"/>
        /// </summary>
        void DispatchBufferToTexture()
        {
            int xThreadGroups = data.Length / THREADS_BUFFER_PASSES;
            bufferToTexture.Dispatch(0, xThreadGroups, 1,1);
            
            //don't need to get data out of buffer here
            _computeBuffer.Dispose();
        }

        
        /// <summary>
        /// dispatches the velocity pass compute shader
        /// <see cref="VelocityPass"/>
        /// </summary>
        void DispatchVelocityPass()
        {
            int xThreadGroups = GasTexture.width / THREADS_TEXTURE_PASSES;
            int yThreadGroups = GasTexture.height / THREADS_TEXTURE_PASSES;
            velocityPass.Dispatch(0, xThreadGroups, yThreadGroups, 1);
        }
        
        
        /// <summary>
        /// dispatches the pressure pass compute shader
        /// <see cref="PressurePass"/>
        /// </summary>
        void DispatchPressurePass()
        {
            
            int xThreadGroups = GasTexture.width / THREADS_TEXTURE_PASSES;
            int yThreadGroups = GasTexture.height / THREADS_TEXTURE_PASSES;
            velocityPass.Dispatch(0, xThreadGroups, yThreadGroups, 1);
        }
        
        
        /// <summary>
        /// dispatches the textureToBuffer compute shader, then copies the data from the
        /// compute buffer before disposing the compute buffer
        /// <see cref="TexToBufferPass"/>
        /// </summary>
        void DispatchTextureToBuffer()
        {
            int xThreadGroups = data.Length / THREADS_BUFFER_PASSES;
            textureToBuffer.Dispatch(0, xThreadGroups, 1,1);
            
            _computeBuffer.GetData(data);
            _computeBuffer.Dispose();
        }

        #endregion


        #region [Extra Stuff]

        private void OnDrawGizmos()
        {
            var positions = new Vector2Int[]
            {
                new Vector2Int(simulationBoundsMin.x, simulationBoundsMin.y),
                new Vector2Int(simulationBoundsMin.x, simulationBoundsMax.y),
                new Vector2Int(simulationBoundsMax.x, simulationBoundsMax.y),
                new Vector2Int(simulationBoundsMax.x, simulationBoundsMin.y),
                new Vector2Int(simulationBoundsMin.x, simulationBoundsMin.y),
            };
            Gizmos.color = Color.red;
            for (int i = 1; i < positions.Length; i++)
            {
                Vector2Int p0 = positions[i - 1];
                Vector2Int p1 = positions[i];
                Vector3 p3 = new Vector3(p0.x, p0.y);
                Vector3 p4 = new Vector3(p1.x, p1.y);
                Gizmos.DrawLine(p3, p4);
            }
        }



      
        
        
        

        [Obsolete("use GasTexture property instead")]
        Texture GetGasTexture()
        {
            if (gasTexture.HasValue)
            {
                var rt = new RenderTexture(velocityFieldTexture.Value.width, velocityFieldTexture.Value.height,
                    velocityFieldTexture.Value.depth);
                rt.enableRandomWrite = true;
                rt.Create();
                gasTexture.Value = rt;
            }

            return gasTexture.Value;
        }

        #region cpu compute

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
        

#endregion

        #endregion
    }
}