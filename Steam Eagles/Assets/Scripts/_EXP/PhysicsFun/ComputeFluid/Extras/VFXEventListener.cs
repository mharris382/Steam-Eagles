using System;
using UniRx;

namespace _EXP.PhysicsFun.ComputeFluid.Extras
{
    public abstract class VFXEventListener<T> : VFXBase
    {
        private void OnEnable()
        {
            MessageBroker.Default.Receive<T>().Where(Filter).TakeUntilDisable(this).Subscribe(OnEvent, HandleException, Cleanup);
        }
        
        
        public abstract bool Filter(T value);
        public abstract void OnEvent(T value);
        public abstract void Cleanup();
        public virtual void HandleException(Exception t) => throw t;
    }
}