using System.Collections.Generic;
using UniRx;
using System;
namespace Buildings
{
    public class BuildingRegistry
    {
        private List<Building> _buildings = new List<Building>();
        private Subject<Building> _buildingAdded = new Subject<Building>();
        public Subject<Building> _buildingRemoved = new Subject<Building>();

        public IObservable<Building> BuildingAdded => _buildingAdded;
        public IObservable<Building> BuildingRemoved => _buildingRemoved;
        public IEnumerable<Building> Buildings => _buildings;

        public void AddBuilding(Building building)
        {
            if (!_buildings.Contains(building))
            {
                _buildings.Add(building);
                _buildingAdded.OnNext(building);
            }
        }
        
        public void RemoveBuilding(Building building)
        {
            if (_buildings.Remove(building)) 
                _buildingRemoved.OnNext(building);
        }
    }
}