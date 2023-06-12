using System;
using UniRx;
namespace SaveLoad
{
    public class EntityLoadSignalBus
    {
        private readonly Subject<Unit> _loadEntites;
        
        public IObservable<Unit> LoadEntities => _loadEntites;

        public EntityLoadSignalBus()
        {
            _loadEntites = new Subject<Unit>();
        }

        public void LoadAllEntities()
        {
            _loadEntites.OnNext(Unit.Default);
        }
    }
}