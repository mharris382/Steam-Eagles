using UnityEngine;

namespace CoreLib
{
    [CreateAssetMenu(menuName = "Shared Variables/Shared string/", fileName = " Shared string", order = -1)]
    public class SharedString : SharedValue<string> { }
}