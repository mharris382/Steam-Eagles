using CoreLib;
using Zenject;

namespace Buildables
{
    public class BuildablesRegistry : Registry<Machine>
    {
        
    }


    public class MachineFactory : PlaceholderFactory<Machine, Machine> { }
}