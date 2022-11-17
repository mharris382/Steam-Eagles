using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public class Rope2D : MonoBehaviour
    {
        public float segmentLength = 0.25f;
        public float ropeLength = 5f;
        public Vector2 connectedAnchorOffset;
        
        public Joint2D ropeStart;
        public Joint2D ropeEnd;
        public Joint2D ropeSegmentPrefab;
    
        
        
        LinkedList<Joint2D> _ropeSegments = new LinkedList<Joint2D>();

        
        void CreateRope()
        {
            var currentLinkedListNode = _ropeSegments.AddFirst(ropeStart);
            int numSegments = Mathf.FloorToInt(ropeLength / segmentLength);
            for (int i = 0; i <= numSegments; i++)
            {
                var newSegment = Instantiate(ropeSegmentPrefab, transform);
                newSegment.transform.SetAsLastSibling();
                switch (newSegment.GetType())
                {
                    case var _ when newSegment is DistanceJoint2D distanceJoint2D:
                        distanceJoint2D.connectedBody = currentLinkedListNode.Value.GetComponent<Rigidbody2D>();
                        distanceJoint2D.distance = i < numSegments ? this.segmentLength : (ropeLength % segmentLength);
                        break;
                }
                newSegment.connectedBody = _ropeSegments.Last.Value.GetComponent<Rigidbody2D>();
                currentLinkedListNode = _ropeSegments.AddLast(newSegment);
            }
            
        }

         
        public void UpdateRope()
        {
            int numSegments = Mathf.FloorToInt(ropeLength / segmentLength);
            int curSegments = transform.childCount;
            if (curSegments > numSegments)
            {
                //need to delete extra segments
            }
            else if(curSegments < numSegments)
            {
                //need to add segments
            }
        }
        
        
    }
}
