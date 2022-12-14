using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GasSim.SimCore.DataStructures;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using World;

namespace Characters.Actions.Selectors
{
    public class CellSelector : MonoBehaviour, ICellSelector
    {
        public Vector2Int size = Vector2Int.one *6;
        public Vector2 spacing = Vector2.one/2f;
        public Vector3 centerOffset = Vector3.zero;
        //private CellHelper _cellHelperPrefab;
        //private List<CellHelper> _cellHelpers;

        private bool doneSetup = false;
        
        public bool Ready => doneSetup;

        public Vector2 SelectorPosition { get; set; }
        public Grid Grid
        {
            get;
            set;
        }
        
        private void Start()
        {
            doneSetup = true;
            // var loadOp = Addressables.LoadAssetAsync<GameObject>("CellHelper");
            // yield return loadOp;
            // if (loadOp.Status == AsyncOperationStatus.Failed)
            // {
            //     Debug.LogError("Couldnt load prefab CelLHelper!");
            // }
            // var prefab = loadOp.Result;
            //
            // _cellHelpers = new List<CellHelper>(size.x * size.y);
            // var positions = GetLocalSpacePositions().ToArray();
            // PriorityQueue<CellHelper> sortedHelpers= new PriorityQueue<CellHelper>(positions.Length);
            //
            //
            // foreach (var localSpacePosition in positions)
            // {
            //     var inst = Instantiate(prefab, transform);
            //     inst.transform.localPosition = localSpacePosition;
            //     sortedHelpers.Enqueue(inst.GetComponent<CellHelper>(), localSpacePosition.sqrMagnitude);
            // }
            //
            // while (!sortedHelpers.IsEmpty)
            // {
            //     _cellHelpers.Add(sortedHelpers.ExtractMin());
            // }
            //
            // doneSetup = true;

        }

        public IEnumerable<Vector3Int> GetSelectedCells()
        {
            if (!doneSetup)
            {
                yield break;
            }

            if (Grid == null)
            {
                Debug.LogWarning("Selector is missing Grid!", this);
                yield break;
            }

            yield return Grid.WorldToCell(SelectorPosition);
            // foreach (var vector3Int in  _cellHelpers.Select(t => t.CellCoordinate))
            // {
            //     yield return vector3Int;
            // }
        }
        
        IEnumerable<Vector3> GetLocalSpacePositions()
        {
            Vector2 totalSize = size * spacing;
            
            Vector2 minPosition = -(totalSize / 2f);
            
            size.x = Mathf.Max(1, size.x);
            size.y = Mathf.Max(1, size.y);
            float xPos = minPosition.x;
            float yPos = minPosition.y;
            float xOffset = spacing.x;
            float yOffset = spacing.y;
            for (int i = 0; i < size.x; i++)
            {
                yPos = minPosition.y;
                xPos += xOffset;
                for (int j = 0; j < size.y; j++)
                {
                    yPos += yOffset;
                    yield return new Vector3(xPos, yPos) + centerOffset;
                }
            }
        }

        private void OnDrawGizmos()
        {
            
            Gizmos.color = Color.Lerp(Color.clear, Color.red, 0.2f);
            foreach (var localSpacePosition in GetLocalSpacePositions())
            {
                var wsPos = transform.TransformPoint(localSpacePosition);
                
                Vector3 cubeSize = spacing;
                
                Gizmos.DrawWireCube(wsPos, cubeSize);
            }
        }

        
    }
}