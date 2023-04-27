using CoreLib;
using UnityEngine.UI;

namespace UI.Extensions
{
    public static class ImageExtensions
    {
        public static void ApplyIcon(this IIconable icon, Image image) => image.sprite = icon.GetIcon();
    }
}