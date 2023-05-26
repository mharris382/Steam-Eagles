using CoreLib;
using CoreLib.Entities;
using CoreLib.Signals;
using UnityEngine;
using Weather.Storms.Views;
using Zenject;

namespace Weather.Storms
{
    public class PCStormSubject : EntityStormSubject
    {
        private readonly GlobalStormConfig _config;
        private readonly PCInstance _pc;
        private readonly IPCTracker _tracker;
        private readonly Camera _camera;
        private readonly EntityInitializer _pcEntityHook;

        public Camera Camera => _camera;
        
        public PCStormSubject(GlobalStormConfig config, PCInstance pc, IPCTracker tracker, GameObject character, Camera camera) : base(character)
        {
            _config = config;
            _pc = pc;
            _tracker = tracker;
            _camera = camera;
            Debug.Assert(_camera != null, "Camera missing from PC Storm Subject", character);
        }
        public class Factory : PlaceholderFactory<PCInstance, IPCTracker, GameObject, Camera, PCStormSubject> { }
        
        public override Bounds SubjectBounds
        {
            get
            {
                var size = Mathf.Pow(_camera.orthographicSize*2, 2) +25;
                var screenSpaceRect = _camera.pixelRect;
                var screenSpaceSize = new Vector2(size * screenSpaceRect.width, size * screenSpaceRect.height);
                var center = _camera.transform.position;
                center.z = 0;
                return new Bounds(center, new Vector3(screenSpaceSize.x, screenSpaceSize.y, 1));
            }
        }

        private StormView _view;

        public override void OnStormAdded(Storm storm)
        {
            var stormView = storm.GetOrCreateView();
            _view = stormView;
            int playerNumber = _pc.PlayerNumber;
            stormView.NotifyPlayerEntered(playerNumber, this);
            _config.Log($"Player {playerNumber} is now effected by storm ");
        }

        public override void OnStormRemoved(Storm storm)
        {
            int playerNumber = _pc.PlayerNumber;
            _config.Log($"Player {playerNumber} no longer effected by storm");
            if (storm.HasView == false )
            {
                if (_view != null)
                {
                    _view.NotifyPlayerExited(_pc.PlayerNumber, this);
                }
                _view= null;
                return;
            }
            var stormView = storm.GetView();
            
            stormView.NotifyPlayerExited(playerNumber, this);
            _view= null;
        }
    }
}