using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings.Tiles
{
    [CreateAssetMenu(fileName = "GasIO", menuName = "Steam Eagles/Tiles/GasIO")]
    public class GasIOTile : TileBase
    {
        [Range(-3, 3), GUIColor(nameof(GuiColor))]
        public int amount = 1; 
        public Color color = Color.green;

        private Color GuiColor
        {
            get
            {
                if(amount == 0)
                    return Color.gray;
                if(amount > 0)
                    return Color.Lerp(new Color(.1f, .7f, .2f), Color.green, -amount / 3f);
                return Color.Lerp(new Color(.7f, .1f, .2f), Color.red, -amount / 3f);
            }
        }
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.color = color;
        }
    }
}