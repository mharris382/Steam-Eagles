using System;
using System.Collections.Generic;
using UniRx;

namespace Tools.BuildTool
{
    public class ToolModeListener
    {
        private IDisposable _disposable;
        private int _currentModeIndex;
        
        private readonly List<string> _modes;
        private readonly ToolControllerBase _controllerBase;
        private readonly IModeNameListener _ui;

        public string CurrentMode
        {
            get => _modes[_currentModeIndex];
            set
            {
                var index = _modes.IndexOf(value);
                if(index == -1)
                    return;
                _currentModeIndex = index;
                OnModeChanged();
            }
        }


        public ToolModeListener(List<string> modes, ToolControllerBase controllerBase)
        {
            _modes = modes;
            _controllerBase = controllerBase;
            if (controllerBase.modeDisplayUI != null)
            {
                _ui = controllerBase.modeDisplayUI.GetComponent<IModeNameListener>();
            }
        }

        public void ListenForInput(Subject<Unit> onToolModeChanged)
        {
            _disposable = onToolModeChanged.StartWith(Unit.Default).Subscribe(_ => OnModeChanged());
        }

        void OnModeChanged()
        {
            if(_currentModeIndex == _modes.Count - 1)
                _currentModeIndex = 0;
            else
                _currentModeIndex++;
            _controllerBase.ToolMode = CurrentMode;
            if (_ui != null)
                _ui.DisplayModeName(CurrentMode);
        }

        public void Dispose()
        {
            _disposable?.Dispose();
            _disposable = null;
        }

        public void SetMode(string lastMode)
        {
            var index = _modes.IndexOf(lastMode);
            if (index == -1)
                return;
            _currentModeIndex = index;
            _controllerBase.ToolMode = CurrentMode;
            if (_ui != null)
                _ui.DisplayModeName(CurrentMode);
        }
    }
}