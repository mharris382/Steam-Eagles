using System.Collections;
using UnityEngine;

namespace World
{
    public class BrushPreview : MonoBehaviour
    {
        public float fadeTime = 1;
        public SpriteRenderer sprite;
        public Grid targetGrid;
        private IEnumerator currentFade;
        
        public void OnSizeChanged(int size)
        {
            StopAllCoroutines();
            StartCoroutine(FadePreview(size));
        }

        private float Alpha
        {
            set
            {
                var color = sprite.color;
                color.a = value;
                sprite.color = color;
            }
            get => sprite.color.a;
        }

        IEnumerator FadePreview(int size)
        {
            sprite.size = new Vector2(size, size) * new Vector2(targetGrid.cellSize.x, targetGrid.cellSize.y);
            Alpha = 1;
            for (float t = 0; t < 1; t+= Time.deltaTime/fadeTime)
            {
                sprite.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Alpha = 1 - t;
                yield return null;
            }
        }
    }
}