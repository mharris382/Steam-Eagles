using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Buildables
{
    public class DebugIOPorts : MonoBehaviour
    {
        public IOPortMode targetMode = IOPortMode.SOURCE;
        [Required]
        public CellDebugger debugger;

        private IOPorts _ports;


        [Inject] public void Install(IOPorts ports)
        {
            _ports = ports;
        }


        private bool Ready => _ports != null;

        private void Update()
        {
            if (!Ready) return;
            debugger.Debug(_ports.Cells(targetMode));
        }
    }
}