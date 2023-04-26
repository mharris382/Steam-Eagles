using System;

namespace Buildables
{
    public class MachineOverlapException : Exception
    {
        public MachineOverlapException(BuildableMachineBase machine, BuildableMachineBase getMachineAtPosition)
        {
            throw new NotImplementedException();
        }
    }
}