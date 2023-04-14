using System;
using UnityEngine;

namespace CoreLib
{
    public readonly struct Physics2DQueryScope : IDisposable
    {
        private readonly bool _queriesStartInColliders;
        private readonly bool _queriesHitTriggers;

        public Physics2DQueryScope(bool? hitTriggers = null, bool? startInColliders = null)
        {
            _queriesHitTriggers = Physics2D.queriesHitTriggers;
            _queriesStartInColliders = Physics2D.queriesStartInColliders;
            if (hitTriggers != null)
            {
                Physics2D.queriesHitTriggers = hitTriggers.Value;
            }
            if(startInColliders != null)
            {
                Physics2D.queriesStartInColliders = startInColliders.Value;
            }
        }

        public void Dispose()
        {
            Physics2D.queriesHitTriggers = _queriesHitTriggers;
            Physics2D.queriesStartInColliders = _queriesStartInColliders;
        }
    }
}