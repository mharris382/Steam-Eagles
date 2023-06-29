using System;
using UniRx;
using UnityEngine;

namespace UI.PlayerGUIs
{
    public class HealthBar : StatBar
    {
        private PlayerCharacterGUIController _playerCharacterGUIController;

        private Health _health;
        
        private void Awake()
        {
            _playerCharacterGUIController = GetComponentInParent<PlayerCharacterGUIController>();
            Debug.Assert(_playerCharacterGUIController != null, "Missing Player Character GUI Controller", this);
        }

        private Health GetHealth()
        {
            if (_playerCharacterGUIController.PlayerCharacter == null)
                return null;
            if (_health != null && _playerCharacterGUIController.PlayerCharacter.gameObject == _health.gameObject)
            {
                return _health;
            }
            _health = _playerCharacterGUIController.PlayerCharacter.GetComponent<Health>();
            return _health;
        }

        public override IReadOnlyReactiveProperty<int> GetMaxValue() => GetHealth().MaxHealthStream;
        public override IReadOnlyReactiveProperty<int> GetCurrentValue() => GetHealth().CurrentHealthStream;
        protected override IObservable<Unit> Redraw() => GetHealth().onRespawn.AsObservable();

        protected override bool IsReady()
        {
            if (_playerCharacterGUIController == null) return false;
            if (_playerCharacterGUIController.PlayerCharacter == null) return false;
            return true;
            throw new NotImplementedException();
        }
    }
}