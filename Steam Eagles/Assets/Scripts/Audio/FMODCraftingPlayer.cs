using System;
using CoreLib.Extensions;
using CoreLib.Structures;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UniRx;
using Zenject;

public class FMODCraftingPlayer : FMODOneShotEventBase, IInitializable, IDisposable
{
    private IDisposable _disposable;
    private readonly EventReference _tileCraftingAction;

    private ReadOnlyReactiveProperty<TileEventInfo> _lastEvent;
    public FMODCraftingPlayer([Inject(Id = FMODEventIDs.TILE_ACTION)] EventReference tileCraftingAction) : base(tileCraftingAction)
    {
    }
    public void Initialize()
    {
        _lastEvent = MessageBroker.Default.Receive<TileEventInfo>().Where(t => !t.isPreview && t.type != CraftingEventInfoType.NO_ACTION).ToReadOnlyReactiveProperty();
        _disposable = _lastEvent.DistinctWhere((t1, t2) => t1.tilePosition == t2.tilePosition).Select(t => t.wsPosition).Subscribe(PlayEventAtPosition);
    }
    protected override void OnPrePlayEvent(EventInstance eventInstance)
    {
        var parameterName = "CraftingActionType";
        var label = GetCraftingEventParam(_lastEvent.Value.type);
        var res =  eventInstance.setParameterByNameWithLabel(parameterName, label);
        if(res != RESULT.OK)throw new Exception($"Failed to set parameter {parameterName} to {label} on event ");
    }
    string GetCraftingEventParam(CraftingEventInfoType infoType)
    {
        switch (infoType)
        {
            case CraftingEventInfoType.DECONSTRUCT:
                return "Deconstruct";
                break;
            case CraftingEventInfoType.BUILD:
                return "Build";
                break;
            case CraftingEventInfoType.DAMAGED:
                return "Damage";
                break;
            case CraftingEventInfoType.SWAP:
                return "Build";
                break;
            case CraftingEventInfoType.REPAIR:
                return "Repair";
                break;
            case CraftingEventInfoType.NO_ACTION:
            default:
                throw new ArgumentOutOfRangeException(nameof(infoType), infoType, null);
        }
    }

    public void Dispose()
    {
        if(_disposable != null) _disposable.Dispose();
        if(_lastEvent != null) _lastEvent.Dispose();
    }
}