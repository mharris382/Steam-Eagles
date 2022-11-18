using System;
using QuikGraph;

namespace Puzzles.GasNetwork
{
    public class PipeEdge : IEdge<PipeNode>
    {
        public PipeNode Source { get; }

        public PipeNode Target { get; }
        
        public FlowDirection FlowDirection { get; set; }

        public PipeEdge(PipeNode source, PipeNode target)
        {
            Source = source;
            Target = target;
        }

        public void ComputeFlowDirection()
        {
            var deltaPosition = Target.Position - Source.Position;
            if (deltaPosition.x > 0)
                FlowDirection = FlowDirection.RIGHT;
            else if (deltaPosition.x < 0)
                FlowDirection = FlowDirection.LEFT;
            else if (deltaPosition.y > 0)
                FlowDirection = FlowDirection.UP;
            else if (deltaPosition.y < 0) FlowDirection = FlowDirection.DOWN;

            var deltaGas = Target.GasStored - Source.GasStored;
            
            if (deltaGas == 0) FlowDirection = FlowDirection.NONE;
            if(deltaGas < 0) InvertFlowDirection();
        }

        private void InvertFlowDirection()
        {
            switch (FlowDirection)
            {
                case FlowDirection.NONE:
                    break;
                case FlowDirection.UP:
                    FlowDirection = FlowDirection.DOWN;
                    break;
                case FlowDirection.DOWN:
                    FlowDirection = FlowDirection.UP;
                    break;
                case FlowDirection.LEFT:
                    FlowDirection = FlowDirection.RIGHT;
                    break;
                case FlowDirection.RIGHT:
                    FlowDirection = FlowDirection.LEFT;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}