using UnityEngine;

namespace CoreLib
{
    [CreateAssetMenu(menuName = "Shared Variables/Create Shared Camera", fileName = "Shared Camera", order = 0)]
    public class SharedCamera : SharedVariable<Camera>{}
}