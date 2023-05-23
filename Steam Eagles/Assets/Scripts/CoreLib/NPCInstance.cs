using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace CoreLib
{
    public class NPCInstance : IDisposable
    {
        private CompositeDisposable _cd;
        public NPCInstance(GameObject character)
        {
            _cd = new CompositeDisposable();
            this.Character = character;
        }

        public GameObject Character { get; private set; }

        public void Dispose()
        {
            _cd?.Dispose();
        }
    }
}