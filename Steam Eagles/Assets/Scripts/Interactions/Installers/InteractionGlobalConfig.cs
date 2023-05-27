using System;
using CoreLib;

namespace Interactions.Installers
{
    [Serializable]
    public class InteractionGlobalConfig : ConfigBase
    {
        public float maxInteractionDistance = 5f;
    }
}