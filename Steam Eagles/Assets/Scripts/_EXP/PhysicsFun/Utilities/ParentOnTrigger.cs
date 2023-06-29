using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhysicsFun
{
    public class ParentOnTrigger : MonoBehaviour
    {
        public bool onlyReParentRootObjects = true;
        public bool filterByTag = false;
        [ShowIf(nameof(filterByTag))]
        public List<string> tagsToFilter = new List<string>(){"Transporter", "Builder"};

        Dictionary<Rigidbody2D, Transform> _rigidbodies = new Dictionary<Rigidbody2D, Transform> ();
        private void OnTriggerEnter2D(Collider2D col)
        {
            var rb = col.attachedRigidbody;
            if(rb == null) return;
            if (rb.isKinematic) return;
            if (onlyReParentRootObjects && rb.transform.parent != null) return;
            if (!HasTag()) return;
            if(_rigidbodies.ContainsKey(rb))return;
            _rigidbodies.Add(rb, rb.transform.parent);
            rb.transform.SetParent(transform, true);
            
            
            bool HasTag()
            {
                if (!filterByTag) return true;
                return tagsToFilter.Contains(rb.tag);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var rb = other.attachedRigidbody;
            if (rb == null) return;
            if (_rigidbodies.ContainsKey(rb) == false) return;
            rb.transform.SetParent(_rigidbodies[rb], true);
            _rigidbodies.Remove(rb);
        }
    }
}