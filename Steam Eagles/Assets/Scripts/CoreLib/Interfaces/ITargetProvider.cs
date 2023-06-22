using System.Collections.Generic;
using CoreLib.Structures;

namespace CoreLib.Interfaces
{
    public interface ITargetProvider
    {
        IEnumerable<Target> GetTargets();
    }
}