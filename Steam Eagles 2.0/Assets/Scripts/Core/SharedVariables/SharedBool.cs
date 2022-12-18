using System;
using UnityEngine;

namespace Core.SharedVariables
{
    [CreateAssetMenu(menuName = "Shared Variables/Shared bool", fileName = " Shared bool", order = -1)]
    public class SharedBool : SharedValue<bool> { }
}