using System;
using CoreLib;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Zenject;

namespace Interactions
{
    public class ElevatorCallButton : Interactable
    {
        [Required]
        public SpriteRenderer buttonSprite;

        [SerializeField] private Sprites sprites;
        [Serializable]
        private class Sprites
        {
            public Sprite defaultSprite;
            public Sprite pressedSprite;
            public Sprite unavailableSprite;

            public Sprite GetSprite(OptionState optionState)
            {
                switch (optionState)
                {
                    case OptionState.SELECTED:
                    case OptionState.CONFIRMED:
                        return pressedSprite;
                    case OptionState.DEFAULT:
                        return defaultSprite;
                    case OptionState.UNAVAILABLE:
                        return unavailableSprite;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(optionState), optionState, null);
                }
            }
        }
        
        public string floorName;
        private ISelectableOption _callElevatorOption;
        private IElevatorMechanism _mechanism;

        [Inject]
        public void InjectMe(IElevatorMechanism mechanism, IActionOptionsProvider optionsProvider)
        {
            _mechanism = mechanism;
            int cnt = 0;
            foreach (var actionOption in optionsProvider.GetOptions())
            {
                if(actionOption.OptionName == floorName)
                {
                    var selectableOption = actionOption as ISelectableOption;
                    _callElevatorOption = selectableOption;
                    Debug.Assert(_callElevatorOption != null, $"{nameof(_callElevatorOption)} != null");
                    _callElevatorOption.OnOptionStateChanged.Subscribe(optionState =>
                        buttonSprite.sprite = sprites.GetSprite(optionState));
                }
                cnt++;
            }
        }
        
        public override async UniTask<bool> Interact(InteractionAgent agent)
        {
            _callElevatorOption.Execute();
            Debug.Assert(_mechanism.IsMoving);
            if (!_mechanism.IsMoving)
                return false;
            await UniTask.WaitUntil(() => !_mechanism.IsMoving);
            return true;
        }
        
        
    }
}