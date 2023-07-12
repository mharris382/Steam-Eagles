using CoreLib.EntityTag;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Buildings.Rooms.Tracking
{
    public class RoomEntities : MonoBehaviour
    {
        private ReactiveCollection<GameObject> _entities = new ReactiveCollection<GameObject>();
        private ReactiveProperty<int> _playerCount = new();
        private ReadOnlyReactiveProperty<bool> _hasPlayersInRoom;

        [ShowInInspector, BoxGroup("Debugging"), ReadOnly, HideInEditorMode]
        public int PlayersInRoom => _playerCount.Value;
        [ShowInInspector, BoxGroup("Debugging"), ReadOnly, HideInEditorMode]
        public bool HasPlayersInRoom => _hasPlayersInRoom?.Value ?? false;
        
        private void Awake()
        {
            var addToCount = _entities.ObserveAdd().Where(t => t.Value.IsPlayer()).Select(_ => 1);
            var removeFromCount = _entities.ObserveRemove().Where(t => t.Value.IsPlayer()).Select(_ => -1);
            addToCount.Merge(removeFromCount).Subscribe(t => _playerCount.Value += t).AddTo(this);
            _hasPlayersInRoom = _playerCount.Select(t => t > 0).ToReadOnlyReactiveProperty();
        }

        public void AddEntity(GameObject entity)
        {
            if(entity == null) return;
            if(_entities.Contains(entity)) return;
            _entities.Add(entity);
        }
        public void RemoveEntity(GameObject entity)
        {
            if(entity == null) return;
            if(!_entities.Contains(entity)) return;
            _entities.Remove(entity);
        }
    }
}