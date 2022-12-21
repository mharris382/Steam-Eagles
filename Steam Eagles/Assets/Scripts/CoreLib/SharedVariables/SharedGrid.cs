using CoreLib;
using UnityEngine;

namespace World
{
    [CreateAssetMenu(menuName = "Shared Variables/Create Shared Grid", fileName = " Shared Grid", order = 0)]
    public class SharedGrid : SharedVariable<Grid> { }
}