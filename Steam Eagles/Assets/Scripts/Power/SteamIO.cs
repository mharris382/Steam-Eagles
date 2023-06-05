using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CoreLib.Extensions;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Power.Steam.Network;
using UniRx;
using UnityEngine;
using Zenject;

namespace Power
{
    public static class SteamIO
    {
        public class Installer : Installer<Installer>
        {
            public override void InstallBindings()
            {
                Container.Bind<SteamProducers>().AsSingle().NonLazy();
                Container.Bind<SteamConsumers>().AsSingle().NonLazy();
                Container.BindFactory<Vector2Int, Func<float>, Action<float>, Producer, Producer.Factory>().AsSingle().NonLazy();
                Container.BindFactory<Vector2Int, Func<float>, Action<float>, Consumer, Consumer.Factory>().AsSingle().NonLazy();
                Container.Bind<Connections>().AsSingle().NonLazy();
            }
        }

        public class Connections
        {
            private readonly NodeRegistry _nodeRegistry;

            struct TrackedPosition : IEquatable<TrackedPosition>
            {
                public Vector2Int Position { get; }

                public TrackedPosition(Vector2Int position)
                {
                    Position = position;
                }

                public TrackedPosition(Vector3Int position)
                {
                    Position = (Vector2Int)position;
                }

                public static implicit operator TrackedPosition(Vector2Int position) => new(position);
                public static implicit operator Vector2Int(TrackedPosition position) => position.Position;
                public static implicit operator TrackedPosition(Vector3Int position) => new(position);
                public static implicit operator Vector3Int(TrackedPosition position) => (Vector3Int)position.Position;

                public bool Equals(TrackedPosition other) => Position.Equals(other.Position);

                public override bool Equals(object obj) => obj is TrackedPosition other && Equals(other);

                public override int GetHashCode() => Position.GetHashCode();
            }


            private readonly Dictionary<TrackedPosition, Vector3Int> _positionToConnected = new();

            [ItemCanBeNull]
            private readonly Dictionary<Vector3Int, HashSet<TrackedPosition>> _connectedToPosition = new();

            private readonly HashSet<Vector2Int> _unconnectedTracked = new();


            public Connections(NodeRegistry nodeRegistry, SteamProducers producers, SteamConsumers consumers)
            {
                _nodeRegistry = nodeRegistry;
                producers.OnSystemAdded.Select(t => t.Item1)
                    .Merge(consumers.OnSystemAdded.Select(t => t.Item1))
                    .Subscribe(AddTrackedPosition);
                producers.OnSystemRemoved.Select(t => t.Item1)
                    .Merge(consumers.OnSystemRemoved.Select(t => t.Item1))
                    .Subscribe(RemoveTrackedPosition);
                _nodeRegistry.OnValueAdded.Select(t => t.Position2D).Subscribe(OnNodeAdded);
                _nodeRegistry.OnValueRemoved.Select(t => t.Position).Subscribe(OnNodeRemoved);
            }

            private void OnNodeAdded(Vector2Int position)
            {
                foreach (var neighbor in position.Neighbors())
                {
                    if (_unconnectedTracked.Contains(neighbor))
                    {
                        ConnectionPosition(neighbor, (Vector3Int)position);
                    }
                }
            }

            private void OnNodeRemoved(Vector3Int position)
            {
                if (_connectedToPosition.ContainsKey(position))
                {
                    foreach (var pos in GetConnectedTrackedPostions(position))
                    {
                        DisconnectPosition(pos);
                    }
                    GetConnectedTrackedPostions(position).Clear();
                }
            }

            private void ConnectionPosition(Vector2Int tracked, Vector3Int position)
            {
                GetConnectedTrackedPostions(position).Add(tracked);
                _positionToConnected[tracked] = position;
            }

            private void DisconnectPosition(Vector2Int tracked, bool tryReconnect = true)
            {
                if (_positionToConnected.ContainsKey(tracked))
                {
                    var prevConnected = _positionToConnected[tracked];
                    GetConnectedTrackedPostions(prevConnected).Remove(tracked);
                    if (tryReconnect && TryGetNewConnection(tracked, out var newConnected))
                    {
                        ConnectionPosition(tracked, newConnected);
                    }
                    else
                    {
                        _positionToConnected.Remove(tracked);
                        _unconnectedTracked.Add(tracked);
                    }
                }
            }

            bool TryGetNewConnection(TrackedPosition tracked, out Vector3Int newConnected)
            {
                newConnected = Vector3Int.zero;
                var adjacent = _nodeRegistry.GetAdjacentComponents(tracked).ToArray();
                if(adjacent.Length > 0)
                {
                    newConnected = adjacent[0].node.Position;
                    return true;
                }
                
                return false;
            }

            private HashSet<TrackedPosition> GetConnectedTrackedPostions(Vector3Int from)
            {
                if (!_connectedToPosition.TryGetValue(from, out var connected))
                {
                    _connectedToPosition.Add(from, connected = new HashSet<TrackedPosition>());
                }
                return connected;
            }

            private void AddTrackedPosition(Vector2Int position)
            {
                if (TryGetNewConnection(position, out var newConnected))
                {
                    ConnectionPosition(position, newConnected);
                }
                else
                {
                    _unconnectedTracked.Add(position);
                }
            }
            
            private void RemoveTrackedPosition(Vector2Int position)
            {
                if(_unconnectedTracked.Contains(position))
                {
                    _unconnectedTracked.Remove(position);
                }
                else
                {
                    DisconnectPosition(position, tryReconnect:false);
                }
            }

            public Vector3Int GetConnectedNode(Vector2Int position)
            {
                if(_positionToConnected.ContainsKey(position))
                {
                    return _positionToConnected[position];
                }
                else if (!TryGetNewConnection(position, out var connected))
                {
                    return connected;
                }

                throw new Exception();
            }

            public bool HasConnection(Vector2Int position)
            {
                try
                {
                    GetConnectedNode(position);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        public class Consumer : ISteamConsumer, IDisposable
        {
            public class Factory : PlaceholderFactory<Vector2Int, Func<float>, Action<float>,  Consumer> { }

            private readonly Vector2Int _cell;

            private readonly IDisposable _disposable;

            private readonly Func<float> _consumptionRateGetter;

            private readonly Action<float> _onSteamConsumed;
            private readonly NodeHandle.Factory _factory;

            public Consumer(Vector2Int cell, Func<float> consumptionRateGetter, Action<float> onSteamConsumed, SteamConsumers consumers, NodeRegistry nodeRegistry, NodeHandle.Factory factory)
            {
                _cell = cell;
                _consumptionRateGetter = consumptionRateGetter;
                _onSteamConsumed = onSteamConsumed;
                _factory = factory;
                var h = _factory.Create((Vector3Int)cell, NodeType.INPUT);
                nodeRegistry.Register(h);
                consumers.AddSystem(cell, this);
                _disposable = Disposable.Create(() =>
                {
                    nodeRegistry.Unregister(h);
                    consumers.RemoveSystem(cell);
                });
            }

            public bool IsActive { get; set; }

            public float GetSteamConsumptionRate() => _consumptionRateGetter();

            public void ConsumeSteam(float amount) => _onSteamConsumed(amount);

            public void Dispose() => _disposable.Dispose();
        }
        public class Producer : ISteamProducer, IDisposable
        {
            public class Factory : PlaceholderFactory<Vector2Int, Func<float>, Action<float>, Producer> { }

            private readonly Vector2Int _cell;
            private readonly Func<float> _productionRateGetter;
            private readonly Action<float> _onSteamProduced;
            private readonly NodeHandle.Factory _factory;
            private readonly IDisposable _disposable;

            public Producer(Vector2Int cell,  Func<float> productionRateGetter,Action<float> onSteamProduced, SteamProducers producers, NodeRegistry nodeRegistry, NodeHandle.Factory factory)
            {
                _cell = cell;
                _productionRateGetter = productionRateGetter;
                _onSteamProduced = onSteamProduced;
                _factory = factory;
                producers.AddSystem(cell, this);
                var h = _factory.Create((Vector3Int)cell, NodeType.INPUT);
                nodeRegistry.Register(h);
                _disposable = Disposable.Create(() =>
                {
                    nodeRegistry.Unregister(h);
                    producers.RemoveSystem(cell);
                });
            }
            public bool IsActive { get; set; }

            public float GetSteamProductionRate() => _productionRateGetter();
            public void ProduceSteam(float amount) => _onSteamProduced(amount);

            public void Dispose() => _disposable?.Dispose();
        }
    }
}