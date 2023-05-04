using System;
using Players.PCController;

public abstract class PCSystem : IDisposable
{
    private readonly PC _pc;

    public PCSystem(PC pc) => _pc = pc;

    public virtual void Dispose()
    {
        
    }
}