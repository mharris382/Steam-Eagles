using Buildings;
using CoreLib;

namespace Buildables
{
    public class MachineHandler<T> : Registry<T>
    {
        public Building building;
        
        public MachineHandler(Building building)
        {
            this.building = building;
        }

        protected override void AddValue(T value)
        {
            
            base.AddValue(value);
        }
    }
}