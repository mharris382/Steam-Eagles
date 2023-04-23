using System;
using UniRx;

namespace Tools.BuildTool
{
    public class ToolActivator
    {
        private readonly ToolControllerBase _controllerBase;
        private IDisposable _disposable;
        private BoolReactiveProperty _isEquipped = new BoolReactiveProperty();
        private BoolReactiveProperty _isInUse = new BoolReactiveProperty();
        private BoolReactiveProperty _isActive = new BoolReactiveProperty();

        public bool IsInUse
        {
            set => _isInUse.Value = value;
        }

        public bool IsEquipped
        {
            set => _isEquipped.Value = value;
        }
            
        public IReadOnlyReactiveProperty<bool> IsActive => _isActive;

        public ToolActivator(ToolControllerBase controllerBase)
        {
            _controllerBase = controllerBase;
            _isEquipped = new BoolReactiveProperty();
            _isInUse = new BoolReactiveProperty();
            _isActive = new BoolReactiveProperty();
            var cd = new CompositeDisposable();
            _isEquipped.Select(t => (t, _isInUse.Value)).Subscribe(t => OnStateChanged(t.t, t.Value)).AddTo(cd);
            _isInUse.Select(t => (_isEquipped.Value, t)).Subscribe(t => OnStateChanged(t.Item1, t.t)).AddTo(cd);
            _disposable = cd;
        }

        void OnStateChanged(bool equipped, bool inUse) => _isActive.Value = equipped && inUse;

        public void Dispose()
        {
            if (_disposable != null)
            {
                _disposable.Dispose();
                _disposable = null;
            }
        }
    }
}