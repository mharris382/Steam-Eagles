using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Tilemaps;

namespace Characters.Actions
{
    public class CellSelector : MonoBehaviour, ICellSelector
    {
        public Vector2Int size = Vector2Int.one *6;
        public Vector2 spacing = Vector2.one/2f;
        public Vector3 centerOffset = Vector3.zero;
        private CellHelper _cellHelperPrefab;
        private List<CellHelper> _cellHelpers;
        private bool doneSetup = false;
        public bool Ready => doneSetup;
        private IEnumerator Start()
        {
            var loadOp = Addressables.LoadAssetAsync<GameObject>("CellHelper");
            yield return loadOp;
            if (loadOp.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogError("Couldnt load prefab CelLHelper!");
            }
            var prefab = loadOp.Result;

            _cellHelpers = new List<CellHelper>(size.x * size.y);
            foreach (var localSpacePosition in GetLocalSpacePositions())
            {
                var inst = Instantiate(prefab, transform);
                inst.transform.localPosition = localSpacePosition;
                var ce = inst.GetComponent<CellHelper>();
                _cellHelpers.Add(ce);
            }

            doneSetup = true;
        }

        public IEnumerable<Vector3Int> GetSelectedCells()
        {
            if (!doneSetup)
            {
                yield break;
            }

            foreach (var vector3Int in  _cellHelpers.Select(t => t.CellCoordinate))
            {
                yield return vector3Int;
            }
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