using System;
using Players.PCController;

public abstract class PCSystem : IDisposable
{
    protected readonly PC _pc;
    
    public PC Pc => _pc;

    public PCSystem(PC pc) => _pc = pc;

    public virtual void Dispose()
    {
        
    }
}