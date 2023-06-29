using System;
using CoreLib;
using CoreLib.Audio;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UniRx;
using Zenject;
using Debug = UnityEngine.Debug;

public class FMODFootstepPlayer : FMODOneShotEventBase, IInitializable, IDisposable
{
    private readonly FMODLabeledParameter _surfaceParameter;
    private IDisposable _disposable;
    private ReadOnlyReactiveProperty<FootstepEventInfo> _lastEvent;

    public FMODFootstepPlayer([Inject(Id = FMODEventIDs.FOOTSTEP)] EventReference eventReference,[Inject(Id = "Surface")] FMODLabeledParameter surfaceParameter) : base(eventReference)
    {
        _surfaceParameter = surfaceParameter;
    }

    public void Initialize()
    {
        _lastEvent = MessageBroker.Default.Receive<FootstepEventInfo>().ToReadOnlyReactiveProperty();
        _disposable = _lastEvent.Select(t => t.Position).Subscribe(PlayEventAtPosition);
    }

    protected override void OnPrePlayEvent(EventInstance eventInstance)
    {
        if (_lastEvent.HasValue && !string.IsNullOrEmpty(_lastEvent.Value.Label))
        {
            var parameterName = "Surface";
            var label = _lastEvent.Value.Label;
            var res = eventInstance.setParameterByNameWithLabel(parameterName, label);
            if (res != RESULT.OK) Debug.LogError($"Failed to set parameter {parameterName} to {label} on event ");
        }
    }

    public void Dispose()
    {
        _disposable?.Dispose();
    }
}