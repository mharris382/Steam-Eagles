using System;
using UniRx;
using UnityEngine;
using Weather.Storms.Views;
using Zenject;

namespace Weather.Storms
{
    public class Storm : IDisposable
    {
        public class Factory : PlaceholderFactory<Bounds, Vector2, Vector2, Storm> { }
        
        private readonly GlobalStormConfig _config;
        
        private Vector2 _falloff;
        private readonly StormView.AltFactory _viewFactory;
        private Bounds _innerBoundsWs;

        public bool IsCompleted
        {
            get;
            private set;
        }
        public static event Action<Storm> OnStormCreated; 
        public static event Action<Storm> OnStormDestroyed;

        public Vector2 Falloff
        {
            get => _falloff;
         
        }
        private StormView _stormView;
        public bool HasView => _stormView != null;
        public StormView GetOrCreateView()
        {
            var view = _stormView ??= _viewFactory.Create();
            view.AssignStorm(this);
            return view;
        }

        public StormView GetView() => _stormView;
        public Storm(GlobalStormConfig config, Bounds innerBoundsWs, Vector2 velocity, Vector2 falloff, StormView.AltFactory viewFactory)
        {
            _config = config;
            _falloff = falloff;
            _viewFactory = viewFactory;

            this.InnerBoundsWs = innerBoundsWs;
            Debug.Assert(InnerBoundsWs.size != Vector3.zero, "Storm bounds cannot be zero");
            this.Velocity = velocity;
            OnStormCreated?.Invoke(this);
        }

        
        /// <summary>
        /// velocity of the center of the storm bounds in world space
        /// </summary>
        public Vector3 Velocity
        { 
            get; 
            set;
        }
        
        /// <summary>
        /// area of the storm in world space
        /// </summary>
        public Bounds InnerBoundsWs
        {
            get => _innerBoundsWs;
            set
            {
                _config.ConstrainStormBounds(ref value);
                _innerBoundsWs = value;
            }
        }

        /// <summary>
        /// edge of the storm in world space
        /// </summary>
        public Bounds OuterBoundsWs
        {
            get
            {
                var bounds = InnerBoundsWs;
                bounds.Expand(_falloff);
                return bounds;
            }
        }

        public Vector2 InnerSize
        {
            set
            {
                var bounds = InnerBoundsWs;
                bounds.size = new Vector3(Mathf.Max(value.x, _config.minStormWidth), Mathf.Max(value.y, _config.minStormHeight), Mathf.Max(bounds.size.z, 1));
                InnerBoundsWs = bounds;
            }
        }

        public Vector2 OuterSize
        {
            set
            {
                var size = value;
                size.x = Mathf.Max(_config.minStormWidth, size.x);
                size.y = Mathf.Max(_config.minStormHeight, size.y);
            }
        }
        
        internal void PhysicsUpdate(float dt)
        {
            var bounds = InnerBoundsWs;
            bounds.center += Velocity * dt;
            InnerBoundsWs = bounds;
        }

        public bool StormContainsPoint(Vector3 posWs) => InnerBoundsWs.Contains(posWs);
        public bool StormContainsPoint(Vector2 posWs) => InnerBoundsWs.Contains(new Vector3(posWs.x, posWs.y, InnerBoundsWs.center.z));
        public bool StormOverlaps(Bounds boundsWs) => InnerBoundsWs.Intersects(boundsWs);


        public void Dispose()
        {
            OnStormDestroyed?.Invoke(this);
            if (HasView) CleanUpView(_stormView);
            IsCompleted = true;
        }

        private void CleanUpView(StormView stormView)
        {
            GameObject.Destroy(stormView.gameObject);
        }
    }
}