using UnityEngine;

namespace Items
{
    
    public abstract class ItemDropper : ScriptableObject
    {
        
        
    }


    public abstract class ItemDropper<T> : ItemDropper where T : MonoBehaviour
    {
        
    }
}