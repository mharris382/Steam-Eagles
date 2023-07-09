using Buildings;
using CoreLib;

namespace Buildables
{
    public class MachineHandler<T> : Registry<T>
    {
        
        public MachineHandler()
        {
            
        }

        protected override void AddValue(T value)
        {
            
            base.AddValue(value);
        }
    }
}