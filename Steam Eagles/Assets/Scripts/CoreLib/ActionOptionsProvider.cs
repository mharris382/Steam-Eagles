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
    
    public interface ISelectableOption : IActionOption
    {
        OptionState OptionState { get; set; }
        System.IObservable<OptionState> OnOptionStateChanged { get; }
    }

    public enum OptionState
    {
        SELECTED,
        CONFIRMED,
        DEFAULT,
        UNAVAILABLE
    }
}