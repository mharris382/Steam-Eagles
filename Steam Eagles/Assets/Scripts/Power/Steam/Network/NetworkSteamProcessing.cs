using System;
using System.Collections.Generic;
using QuikGraph;
using UnityEngine;
using UniRx;
namespace Power.Steam.Network
{
    public class NetworkSteamProcessing  : ISteamProcessing
    {
        public SteamConsumers SteamConsumers { get; }

        class SteamNodeState
        {
            public Vector2Int Position { get; }
            public float Pressure { get;set; }
            public float Temperature { get;set; }

            public SteamNodeState(Vector2Int position, float pressure, float temperature)
            {
                Position = position;
                Pressure = pressure;
                Temperature = temperature;
            }

            public Color ToColor()
            {
                var color = Color.white;
                color.r = Pressure;
                color.g = Temperature;
                return color;
            }
        }


        private Dictionary<Vector2Int, SteamNodeState> _steamNodeStates = new();
        private readonly INetworkTopology _networkTopology;
        private readonly SteamProducers _steamProducers;
        private readonly CoroutineCaller _coroutineCaller;
        private readonly SteamConsumers _steamConsumers;

        private GridGraph<NodeHandle> _gridGraph;

        public NetworkSteamProcessing(INetworkTopology networkTopology, GridGraph<NodeHandle> gridGraph, SteamProducers steamProducers, SteamConsumers steamConsumers, CoroutineCaller coroutineCaller)
        {
            SteamConsumers = steamConsumers;
            _networkTopology = networkTopology;
            _steamProducers = steamProducers;
            _gridGraph = gridGraph;
            _coroutineCaller = coroutineCaller;
            
            _gridGraph.OnNodeAdded.Subscribe(OnNodeAdded);
            _gridGraph.OnNodeRemoved.Subscribe(OnNodeRemoved);
            _gridGraph.OnEdgeAdded.Subscribe(OnEdgeAdded);
            _gridGraph.OnEdgeRemoved.Subscribe(OnEdgeRemoved);
        }
        private void OnEdgeRemoved(TaggedUndirectedEdge<GridNode,NodeHandle> o)
        {
            var p0 =(Vector2Int)o.Source.Position;
            var p1 =(Vector2Int)o.Target.Position;
            if (HasPosition(p0))
            {
                Debug.LogError(" new NotImplementedException()");
            }
            if (HasPosition(p1))
            {
                Debug.LogError(" new NotImplementedException()");
            }
        }
        private void OnEdgeAdded(TaggedUndirectedEdge<GridNode,NodeHandle> o)
        {
            Debug.LogError(" new NotImplementedException()");
        }
        private void OnNodeAdded(GridNode o)
        {
            var steamState = new SteamNodeState((Vector2Int)o.Position, 0, 0);
            _steamNodeStates.Add(steamState.Position, steamState);
        }
        private void OnNodeRemoved(GridNode o)
        {
            _steamNodeStates.Remove((Vector2Int)o.Position);
        }

        public void UpdateSteamState(float deltaTime)
        {
            throw new NotImplementedException();
        }

        public bool HasPosition(Vector2Int position)
        {
            return _steamNodeStates.ContainsKey(position);
        }

        public float GetSteamFlowRate(Vector2Int p0, Vector2Int p1)
        {
            throw new NotImplementedException();
        }

        public float GetPressureLevel(Vector2Int position)
        {
            if (HasPosition(position))
            {
                return _steamNodeStates[position].Pressure;
            }
            return 0;
        }

        public float GetTemperature(Vector2Int position)
        {
            if (HasPosition(position))
            {
                return _steamNodeStates[position].Temperature;
            }

            return 0;
        }

        private void SetTemperature(Vector2Int position, float temp)
        {
            if (!HasPosition(position))
            {
                throw new Exception();
            }            
            var state = _steamNodeStates[position];
            state.Temperature = temp;
        }
        private void SetPressure(Vector2Int position, float pressure)
        {
            if (!HasPosition(position))
            {
                throw new Exception();
            }            
            var state = _steamNodeStates[position];
            state.Pressure = pressure;
        }
        
        public bool IsBlocked(Vector2Int position)
        {
            return HasPosition(position);
        }

        public void LoadSteamStateForTexture(BoundsInt cellBounds, Texture2D saveTexture)
        {
            int loadedCellCount = 0;
            for (int x = 0; x < cellBounds.size.x; x++)
            {
                var cellX = x + cellBounds.xMin;
                for (int y = 0; y < cellBounds.size.y; y++)
                {
                    var cellY = y + cellBounds.yMin;
                    var cell = new Vector2Int(cellX, cellY);
                    if (HasPosition(cell))
                    {
                        var color = saveTexture.GetPixel(x, y);
                        var pressure = color.r;
                        var temperature = color.g;
                        SetPressure(cell, 0);
                        SetTemperature(cell, 0);
                        loadedCellCount++;
                    }
                }
            }
            Debug.Log($"Loaded {loadedCellCount} cells");
        }
    }
}