using System;
using System.Collections.Generic;
using Buildables;
using Buildings;
using UniRx;
using UnityEngine;
using Zenject;

namespace Buildables
{
    public class BMachine : IDisposable
    {
        public class Factory : PlaceholderFactory<BuildableMachineBase, Vector2Int, BMachine> { }
        
        private readonly Building _building;
        private readonly List<Vector2Int> positions;
        private readonly IDisposable _destroyBMachine;

        public BuildableMachineBase Machine { get; }
        public IEnumerable<Vector2Int> Cells => positions;
        public BMachine(BuildableMachineBase machine, Vector2Int placement, Building building, BMachineHelper helper)
        {
            _building = building;
            Machine = machine;
            positions = new List<Vector2Int>();
            positions.AddRange(helper.GetMachineCells(machine, placement));

            _building.Map.BlockCells(positions, BuildingLayers.SOLID);
            _building.Map.BlockCells(positions, BuildingLayers.PIPE);
            
            Subject<Unit> onDispose = new();
            onDispose.Subscribe(_ => _building.Map.UnblockCells(positions, BuildingLayers.SOLID));
            onDispose.Subscribe(_ => _building.Map.UnblockCells(positions, BuildingLayers.PIPE));
            onDispose.Subscribe(_ => Machine.DestroyMachine());
            
            _destroyBMachine = Disposable.Create(() =>
            {
                onDispose?.OnNext(Unit.Default);
                onDispose?.OnCompleted();
                onDispose?.Dispose();
            });
        }

        public void Dispose()
        {
            _destroyBMachine?.Dispose();
        }
    }
}