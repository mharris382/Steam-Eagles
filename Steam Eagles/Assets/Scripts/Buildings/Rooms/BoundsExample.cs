using UnityEngine;

namespace Buildings.Rooms
{
    public class BoundsExample : MonoBehaviour
    {
        public Bounds bounds { get { return m_Bounds; } set { m_Bounds = value; } }
        [SerializeField]
        private Bounds m_Bounds = new Bounds(Vector3.zero, Vector3.one);
    }
}