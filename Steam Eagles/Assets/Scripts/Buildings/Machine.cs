using UnityEngine;
using Zenject;

namespace CoreLib
{
    public abstract class Machine : MonoBehaviour
    {
        public class Factory : PlaceholderFactory<Machine, Machine>{
            
        }
    }


}