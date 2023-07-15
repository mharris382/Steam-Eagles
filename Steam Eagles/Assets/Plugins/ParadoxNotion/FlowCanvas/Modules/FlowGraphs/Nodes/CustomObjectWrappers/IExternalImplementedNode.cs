namespace FlowCanvas.Nodes
{
    ///<summary>Implement in a UnityObject MonoBehaviour/ScriptableObject to make it possible to add as a node in a flowScript by forwarding port registration.</summary>
    public interface IExternalImplementedNode
    {
        void RegisterPorts(FlowNode parent);
    }
}