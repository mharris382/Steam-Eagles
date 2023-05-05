using System.Collections.Generic;

namespace CoreLib
{
    public interface IActionOption
    {
        string OptionName {get;}
        bool IsAvailable {get;}
        void Execute();
    }
    
    public interface IActionOptionsProvider
    {
        IEnumerable<IActionOption> GetOptions();
    }
}