using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
namespace Players
{
    public abstract class PlayerSubsystem : MonoBehaviour
    {
        [Required]
        [SerializeField] private Player player;
        
        
        
    }
}