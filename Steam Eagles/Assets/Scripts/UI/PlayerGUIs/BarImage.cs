using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI.PlayerGUIs
{
    [RequireComponent(typeof(Image))]
    public class BarImage : MonoBehaviour
    {
        private Image _img;
        public Image img => _img ? _img : _img = GetComponent<Image>();

       [Required, PreviewField,SerializeField] private Sprite fullSprite;
       [Required, PreviewField,SerializeField] private Sprite halfSprite;
       [Required, PreviewField,SerializeField] private Sprite emptySprite;

       public void Set(float full)
       {
            if(full <= 0)
                img.sprite = emptySprite;
            else if (full <= 0.5f)
                img.sprite = halfSprite;
            else
                img.sprite = fullSprite;
       }
    }
}