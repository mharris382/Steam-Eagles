using System;
using UnityEngine;

namespace Tools.DestructTool
{
    public partial class DestructToolController
    {
        [Serializable]
        public class DestructionConfig
        {
            public LayerMask destructibleLayers; // = LayerMask.GetMask("Solids", "Pipes", "Machines");
            public float radius = 1f;
            public float rate = 1f;
            public bool sortByDistance = true;
            [Tooltip(
                "If the destuctor does not receive a hit for this amount of time, it will reset the time until next destruction")]
            public float destructionResetTimer = 0.5f;
        }
    }
}