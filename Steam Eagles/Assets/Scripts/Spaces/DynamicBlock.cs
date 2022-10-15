using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Steam Eagles/Static Block")]
public class DynamicBlock : ScriptableObject
{
    public StaticBlock linkedStaticBlock;
    
    [SerializeField] private DynamicBlockInstance _prefab;

    
    
    
    public UnityEvent<DynamicBlockInstance> onInstanceSpawned;
    
    public DynamicBlockInstance SpawnDynamicBlockInstance(Vector3 position, float rotation)
    {
        var inst = new GameObject($"{name} (Instance)", typeof(Rigidbody2D)).AddComponent<DynamicBlockInstance>();
        inst.Block = this;
        return inst;
    }
}