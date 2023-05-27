using CoreLib;
using Zenject;

namespace Buildables
{
    public class BuildablesRegistry : RegistryBase<Machine>
    {
        
    }


    public class MachineFactory : PlaceholderFactory<Machine, Machine> { }
}