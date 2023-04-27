using Power.Steam;
using UnityEngine;

namespace Buildables.Parts
{
    public class GasOutputAttachment : PipeAttachmentPart
    {
        public float amount = 10;
        public SupplierNode supplierNode { get; set; }
        public override void ConnectToNetwork(SteamNetworks.SteamNetwork network, Vector3Int vector3Int)
        {
           // supplierNode = network.AddSupplierAt(vector3Int);
           // supplierNode.AmountSuppliedPerUpdate = amount;
        }
    }
}