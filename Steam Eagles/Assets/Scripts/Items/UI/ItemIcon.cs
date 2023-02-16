using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Items.UI
{
    public class ItemIcon : UIItemElement
    {
        public Image icon;


        protected override void Awake()
        {
            base.Awake();
        }

        public override void OnItemChanged(ItemBase item)
        {
            icon.sprite = item.icon;
        }
    }
}