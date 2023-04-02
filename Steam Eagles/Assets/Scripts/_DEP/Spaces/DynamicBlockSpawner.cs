using UnityEngine;
using Random = UnityEngine.Random;

namespace Spaces
{
    public class DynamicBlockSpawner : MonoBehaviour
    {
        public DynamicBlock block;
    
        public float spawnRadius = 0;
        public Vector2 spawnRotation = new Vector2(0, 360);
        public bool hide;
    
    
    
    
        public void SpawnBlock(Vector3 position)
        {
            var pos = position;
            if (spawnRadius > 0)
            {
                var q = Quaternion.Euler(0, 0, Random.Range(0, 360));
                var offset = q * (Vector3.right * spawnRadius);
                pos += offset;
            }
            var blockInst = block.SpawnDynamicBlockInstance(pos, Random.Range(spawnRotation.x, spawnRotation.y));
            if (hide)
            {
                blockInst.gameObject.hideFlags = HideFlags.HideInHierarchy;
            }
        }


        // private void Update()
        // {
        //     if (Input.GetKeyDown(KeyCode.Space))
        //     {
        //         var cam = Camera.main;
        //         var wp = cam.ScreenToWorldPoint(Input.mousePosition);
        //         wp.z = 0;
        //         SpawnBlock(wp);
        //     }
        // }
    }
}