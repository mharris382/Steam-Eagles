using Power.Steam;
using UnityEngine;

namespace Buildables.Parts
{
    public class GasInputAttachment : PipeAttachmentPart
    {
        public float amount = 1000;
        public ConsumerNode consumerNode { get; set; }
        public override void ConnectToNetwork(SteamNetworks.SteamNetwork network, Vector3Int vector3Int)
        {
            //consumerNode = network.AddConsumerAt(vector3Int);
            //consumerNode.MaxConsumptionPerUpdate = amount;
        }
    }
}