using System.Collections.Generic;

namespace CoreLib
{
    public interface IElevatorMechanism
    {
        bool MoveToFloor(int floor);
        bool IsMoving { get; }
        IEnumerable<string> GetFloorNames();
    }
    
    
}