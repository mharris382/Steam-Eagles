using UnityEngine;

namespace Core.SharedVariables
{
    public abstract class SharedComponent<T> : SharedReference<T> where T : Component{}
}