using Sirenix.OdinInspector;
using UnityEngine;

namespace Buildings.Rooms
{
    public class TexturePreview : MonoBehaviour
    {
        [ShowInInspector, PreviewField(400, ObjectFieldAlignment.Center, FilterMode = FilterMode.Point)]
        private  Texture2D _texture2D;

        public void Set(Texture2D texture2D)
        {
            _texture2D = texture2D;
        }
    }
}