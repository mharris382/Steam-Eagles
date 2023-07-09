using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Buildables
{
    public class MachineDebugger : MonoBehaviour
    {
        MachineHandlers _handlers;
        bool ShowDebug => _handlers != null;
        
        [Inject]void Inject(MachineHandlers handlers)
        {
            _handlers = handlers;
        }


        [ShowInInspector, ShowIf(nameof(ShowDebug))]  private int Pumps => !ShowDebug ? 0 : _handlers.hyperPumps.ListCount;
        [ShowInInspector, ShowIf(nameof(ShowDebug))]  private int Generators => !ShowDebug ? 0 : _handlers.generators.ListCount;
        [ShowInInspector, ShowIf(nameof(ShowDebug))]  private int Turbines => !ShowDebug ? 0 : _handlers.turbines.ListCount;
        [ShowInInspector, ShowIf(nameof(ShowDebug))]  private int Vents => !ShowDebug ? 0 : _handlers.vents.ListCount;
    }
}