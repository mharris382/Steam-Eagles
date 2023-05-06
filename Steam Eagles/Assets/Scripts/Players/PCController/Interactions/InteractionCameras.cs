using UnityEngine;

namespace Players.PCController.ParallaxSystems
{
    public class InteractionCameras : PCRegisteredCollection<VCam>
    {
        protected override VCam CreateCopy(VCam original, int playerNumber)
        {
            var copy = GameObject.Instantiate(original, original.transform.parent);
            copy.gameObject.layer = LayerMask.NameToLayer($"P{playerNumber+1}");
            return copy;
        }
    }
}