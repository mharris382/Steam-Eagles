using Tools.BuildTool;
using UnityEngine;
using Zenject;

namespace Players.PCController.Tools
{
    public class PCToolSystem : PCSystem
    {
        public class Factory : PlaceholderFactory<PC, PCToolSystem> , ISystemFactory<PCToolSystem> { }
        private ToolSwapController _toolSwapController;
        public PCToolSystem(PC pc) : base(pc)
        {
            _toolSwapController = pc.PCInstance.character.GetComponent<ToolSwapController>();
            
        }

        public void Tick()
        {
            
        }

        class ToolFactory : IPCToolFactory
        {
            public ToolFactory(DiContainer container)
            {
                
            }
            public GameObject Create(PC param1, GameObject param2)
            {
                throw new System.NotImplementedException();
            }
        }
    }


    public interface IPCToolFactory : IFactory<PC, GameObject, GameObject> { }
    public class PCToolFactory : PlaceholderFactory<PC, GameObject, GameObject> { }
    
    
}