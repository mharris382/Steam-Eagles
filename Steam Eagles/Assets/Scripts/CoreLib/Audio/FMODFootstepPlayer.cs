using System;
using CoreLib;
using FMODUnity;
using UniRx;
using Zenject;

public class FMODFootstepPlayer : FMODOneShotEventBase, IInitializable, IDisposable
{
    private IDisposable _disposable;
    public FMODFootstepPlayer([Inject(Id = FMODEventIDs.FOOTSTEP)] EventReference eventReference) : base(eventReference) { }

    public void Initialize()
    {
        _disposable = MessageBroker.Default.Receive<FootstepEventInfo>().Select(t => t.Position).Subscribe(PlayEventAtPosition);
    }

    public void Dispose()
    {
        _disposable?.Dispose();
    }
}