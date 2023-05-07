using System;
using System.Collections.Generic;
using System.Linq;
using CoreLib;
using UniRx;
using UnityEngine;
using Zenject;

namespace Interactions
{
   
    public class InteractionOptionHandler
    {
        const float TIME_TO_CHANGE_OPTION = 0.25f;
        private readonly InteractionAgent _agent;
        private readonly ElevatorController _controller;
        private readonly List<ISelectableOption> _actionOptions;

        private int _prevOptionIndex = -1;
        private int _currentOptionIndex = 0;
        private float _timeSinceOptionChanged = 0;
        private Subject<(int prevOption, int nextOption)> onSelectedOptionChanged = new Subject<(int prevOption, int nextOption)>();
        public IObservable<(int prevOption , int nextOption)> OnSelectedOptionChanged => onSelectedOptionChanged;
        
        public ISelectableOption CurrentOption => _actionOptions[_currentOptionIndex];
        public ISelectableOption PrevOption => _prevOptionIndex == -1 ? null : _actionOptions[_currentOptionIndex];
        
        public IEnumerable<IActionOption> ActionOptions => _actionOptions;

        public InteractionOptionHandler(InteractionAgent agent, ElevatorController controller, IActionOptionsProvider actionOptionsProvider)
        {
            _agent = agent;
            _controller = controller;
            _actionOptions = actionOptionsProvider.GetOptions().Select(t => t as ISelectableOption).ToList();
             _currentOptionIndex = 0;
             UpdateOptions();
        }

        public void SelectNextOption()
        {
            _prevOptionIndex = _currentOptionIndex;
            if(_currentOptionIndex < _actionOptions.Count - 1)
                _currentOptionIndex++;
            else
                _currentOptionIndex = 0;
            UpdateOptions();
        }

        public void SelectPrevOption()
        {
            _prevOptionIndex = _currentOptionIndex;
            if(_currentOptionIndex > 0)
                _currentOptionIndex--;
            else
                _currentOptionIndex = _actionOptions.Count - 1;
            UpdateOptions();
        }
        
        void UpdateOptions()
        {
            foreach (var selectableOption in _actionOptions)
            {
                selectableOption.OptionState = OptionState.DEFAULT;
            }
            CurrentOption.OptionState = OptionState.SELECTED;
        }

        public void ConfirmOption()
        {
            CurrentOption.Execute();
            CurrentOption.OptionState = OptionState.CONFIRMED;
        }
        
        public void Update()
        {
            if (Time.time - _timeSinceOptionChanged < TIME_TO_CHANGE_OPTION)
            {
                return;
            }
            
            int yInput = _agent.SelectInputY;
            if (_controller.flipInput) yInput *= -1;
            if(yInput > 0)
            {
                SelectPrevOption();
                _timeSinceOptionChanged = Time.time;
            }
            else if(yInput < 0)
            {
                SelectNextOption();
                _timeSinceOptionChanged = Time.time;
            }
        }
        
        public class Factory : PlaceholderFactory<InteractionAgent, InteractionOptionHandler>{ }
    }
}