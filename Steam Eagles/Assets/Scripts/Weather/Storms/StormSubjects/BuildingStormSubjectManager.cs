using System;
using System.Collections.Generic;
using Buildings;
using UniRx;
using Zenject;

namespace Weather.Storms
{
    public class BuildingStormSubjectManager : StormSubjectManagerBase<Building>
    {
        private readonly BuildingRegistry _buildingRegistry;
        private readonly BuildingStormSubject.Factory _buildingStormSubjectFactory;

        
        public BuildingStormSubjectManager(BuildingRegistry buildingRegistry, BuildingStormSubject.Factory buildingStormSubjectFactory)
        {
            _buildingRegistry = buildingRegistry;
            _buildingStormSubjectFactory = buildingStormSubjectFactory;
        }

      

        private BuildingStormSubject CreateBuildingSubject(Building building)
        {
            return _buildingStormSubjectFactory.Create(building);
        }
        protected override void ListenForDynamicStormSubjects(Action<IStormSubject> onStormSubjectAdded, Action<IStormSubject> onStormSubjectRemoved, CompositeDisposable cd)
        {
            _buildingRegistry.BuildingAdded.Select(CreateBuildingSubject).Subscribe(onStormSubjectAdded).AddTo(cd);
            _buildingRegistry.BuildingRemoved.Where(HasSubjectFor).Select(GetSubjectFor).Subscribe(onStormSubjectRemoved).AddTo(cd);
        }
        
        protected override IStormSubject CreateStormSubject(Building subject) => _buildingStormSubjectFactory.Create(subject);
        protected override IEnumerable<Building> GetInitialSubjects() => _buildingRegistry.Buildings;
        
    }
    
    public class BuildingStormSubject : EntityStormSubject
    {
        public class Factory : PlaceholderFactory<Building, BuildingStormSubject> {}

        public BuildingStormSubject(Building building) : base(building.gameObject) { }
    }

}