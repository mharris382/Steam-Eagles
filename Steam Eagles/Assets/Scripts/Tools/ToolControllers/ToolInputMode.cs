
using Tools.BuildTool;

namespace Tools.ToolControllers
{
    public interface IToolCommand
    {
        void Execute();
        bool IsFinished();
    }

    public abstract class ToolState
    {
        protected readonly ToolControllerBase toolController;

        public ToolState(ToolControllerBase toolController)
        {
            this.toolController = toolController;
        }

        public virtual void OnEnter()
        {

        }

        public virtual void OnExit()
        {
            
        }
        public virtual void Update()
        {
            
        }
    }


    public class SingleClickInputState
    {
        
    }
}