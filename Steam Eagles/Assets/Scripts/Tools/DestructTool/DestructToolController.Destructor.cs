using System;
using System.Linq;
using CoreLib.Interfaces;
using UniRx;
using UnityEngine;

namespace Tools.DestructTool
{
    public partial class DestructToolController
    {
        public class Destructor 
        {
            private readonly IDestruct _destructable;
            private readonly DestructToolController _tool;

            private float _lastHitTime;
            private float _remainingTimeTillNextDestruct;
            private Subject<DestructParams> _onDestruct = new Subject<DestructParams>();

            public Destructor(IDestruct destructable, DestructToolController tool)
            {
                _destructable = destructable;
                _tool = tool;
                _remainingTimeTillNextDestruct = tool.config.rate;
                _onDestruct.Buffer(TimeSpan.FromSeconds(tool.config.rate))
                    .Where(t => t.Count > 0)
                    .Select(t=> t.First())
                    .Subscribe(t =>
                    {
                        Debug.Log($"Got hit on {_destructable} with {t}");
                        _destructable.TryToDestruct(t);
                    });
            }

            public void OnHit(float dt, DestructParams dparams)
            {
                _onDestruct.OnNext(dparams);

                // _lastHitTime = Time.realtimeSinceStartup;
                // _remainingTimeTillNextDestruct -= dt;
                // if (_remainingTimeTillNextDestruct > 0)
                //     return;
                //
                // _remainingTimeTillNextDestruct += _tool.config.rate;
                // _destructable.TryToDestruct(dparams);
            }
        }
    }
}