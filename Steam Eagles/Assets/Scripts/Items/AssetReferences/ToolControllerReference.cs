using Utilities.AddressablesUtils;

namespace Items
{
    [System.Serializable]
    public class ToolControllerReference : ComponentReference<ToolControllerBase>
    {
        public ToolControllerReference(string guid) : base(guid)
        {
        }
    }
}