using UnityEngine;
using UnityEngine.Tilemaps;

namespace PhysicsFun.Airships
{
    [CreateAssetMenu(menuName = "Steam Eagles/Counter Weight Creator")]
    public class CounterWeightCreator : ScriptableObject
    {
        public Vector3 spawnOffset = Vector3.down;
        public CounterWeight counterWeightPrefab;
        public CounterWeightAttachment counterWeightAttachmentPrefab;


        public bool TryAttachCounterWeight(Tilemap tilemap, Vector3Int cell)
        {
            if (!tilemap.HasTile(cell))
            {
                return false;
            }

            TilemapAttachments tilemapAttachments;
            if(!tilemap.gameObject.TryGetComponent<TilemapAttachments>(out  tilemapAttachments))
            {
                tilemapAttachments = tilemap.gameObject.AddComponent<TilemapAttachments>();
            }
            
            var attachment = tilemapAttachments.GetAttachmentAt<CounterWeightAttachment>(cell);
            if (attachment == null)
            {
                var attachmentInstance = Instantiate(counterWeightAttachmentPrefab, tilemap.CellToWorld(cell), Quaternion.identity);
                attachmentInstance.transform.SetParent(tilemap.transform);
                if(!tilemapAttachments.TryAttach(attachmentInstance, cell))
                {
                    Debug.LogError($"Unable to attach {attachmentInstance.name} to {tilemap.name} at {cell}", this);
                    Destroy(attachmentInstance.gameObject);
                    return false;
                }
                attachment = attachmentInstance;
            }
            var weightInstance = Instantiate(counterWeightPrefab, attachment.transform.position + spawnOffset, Quaternion.identity);
            attachment.AddWeight(weightInstance);
            return false;
        }
    }
}