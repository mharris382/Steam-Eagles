using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class SelectUnderMouse : MonoBehaviour
    {
        public EventSystem eventSystem;
        public GraphicRaycaster raycaster;
    
        private void Update()
        {
            var mousePos = Input.mousePosition;
            var pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = mousePos;
            var results = new List<RaycastResult>();
            raycaster.Raycast(pointerEventData, results);
            foreach (var result in results)
            {
                if (result.gameObject.GetComponent<Selectable>() != null)
                {
                    result.gameObject.GetComponent<Selectable>().Select();
                    break;
                }
            }
        }
    }
}