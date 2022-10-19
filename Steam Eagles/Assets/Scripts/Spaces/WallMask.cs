using System.Linq;
using UnityEngine;

namespace Spaces
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class WallMask : MonoBehaviour
    {
        private SpriteRenderer _sr;
        private SpriteRenderer sr => _sr == null ? (_sr = GetComponent<SpriteRenderer>()) : _sr;


        public Sprite maskSprite;
        public Vector2 spriteSize = Vector2.one;
        public bool copySize;

        public bool filterOrder=true;
        public bool filterLayer=false;
        public bool filterZPosition=true;
        [SerializeField]
        private SpriteMask mask;

        public bool hideMaskInHeirarchy = true;

        public Transform filterTargets;
        [Tooltip("If enabled all child sprites will be effected by the mask, otherwise only the direct children will be effected")]
        public bool filterChildSpritesOnly;

        
        private SpriteMask Mask
        {
            get
            {
                if (mask == null)
                {
                    mask = GetComponentInChildren<SpriteMask>();
                }

                if (mask == null)
                {
                    var go = new GameObject($"{name} Mask", typeof(SpriteMask));
                    go.transform.SetParent(transform, false);
                    go.transform.localScale = Vector3.zero;
                    mask = go.GetComponent<SpriteMask>();
                }
                return mask;
            }
        }

        
        
        
        private void OnDrawGizmos()
        {
            if (mask == null)
            {
                
            }
            UpdateMask(sr, Mask);
        }

        void UpdateMask(SpriteRenderer sr, SpriteMask mask)
        {
            Mask.gameObject.hideFlags = hideMaskInHeirarchy ? (HideFlags.HideInHierarchy | HideFlags.HideInInspector) : HideFlags.None;
            Mask.sprite = this.maskSprite;
            if (filterLayer || filterOrder)
            {
                Mask.isCustomRangeActive = true;
                float minZ = float.MaxValue;
                float maxZ = float.MinValue;

                int minLayer = int.MaxValue;//sr.sortingLayerID;
                int maxLayer = int.MinValue;//sr.sortingLayerID;
                
                int minOrder = int.MaxValue;// sr.sortingOrder;
                int maxOrder =  int.MinValue;//sr.sortingOrder;
                
                if (filterTargets != null)
                {
                    Transform[] targets = null;
                    if (filterChildSpritesOnly)
                    {
                        targets = new Transform[filterTargets.childCount];
                        for (int i = 0; i < filterTargets.childCount; i++)
                        {
                            targets[i] = filterTargets.GetChild(i);
                        }
                    }
                    else
                    {
                        targets = filterTargets.GetComponentsInChildren<SpriteRenderer>().Select(t => t.transform).ToArray();
                    }
                    for (int i = 0; i < targets.Length; i++)
                    {
                        var ct = targets[i];
                            
                        if (ct.position.z < minZ) minZ = ct.position.z;
                        if (ct.position.z > maxZ) maxZ = ct.position.z;
                            
                        var child =ct.GetComponent<SpriteRenderer>();
                        if (child == null) continue;

                        if (child.sortingOrder > maxOrder)
                            maxOrder = child.sortingOrder;
                        if (child.sortingOrder < minOrder)
                        {
                            minOrder = child.sortingOrder;
                        }

                        if (child.sortingLayerID > maxLayer)
                        {
                            maxLayer = child.sortingLayerID;
                        }

                        if (child.sortingLayerID < minLayer)
                        {
                            minLayer = child.sortingLayerID;
                        }
                    }

                    if (filterOrder)
                    {
                        Mask.backSortingOrder = minOrder-1;
                        Mask.frontSortingOrder = maxOrder+1;
                    }

                    if (filterLayer)
                    {
                        Mask.frontSortingLayerID = maxLayer;
                        
                        Mask.backSortingLayerID = minLayer;
                    }

                }
            }
            else
            {
                Mask.isCustomRangeActive = false;
            }
            if (copySize)
            {
                var scaleWidth =  maskSprite.textureRect.width/maskSprite.pixelsPerUnit;
                var scaleHeight = maskSprite.textureRect.height/maskSprite.pixelsPerUnit;
                scaleHeight *= (sr.size.y);
                scaleWidth *= (sr.size.x);
                Mask.transform.localScale = new Vector3(scaleWidth, scaleHeight);
            }
        }
    }
}