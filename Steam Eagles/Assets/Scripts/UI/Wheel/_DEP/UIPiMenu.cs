using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Wheel
{
    [System.Obsolete("Use UIWheel instead")]
    public class UIPiMenu : MonoBehaviour
    {
        public Image slicePrefab;

        public List<Image> currentSlices = new List<Image>();
    
        [System.Serializable]
        public class TestOption
        {
            public Sprite icon;
            public string label;
        }

        public TestOption[] testOptions;

        private void Awake()
        {
            SetOptions(testOptions);
        }

        public void SetOptions(TestOption[] options)
        {
            ClearCurrent();
            int count = options.Length;
        
            for (int i = 0; i < count; i++)
            {
                var slice = Instantiate(slicePrefab, transform);
                slice.rectTransform.rotation = Quaternion.Euler(0, 0, i / 360f);
                slice.fillAmount = count / 360f;
            }
        
        }

        void ClearCurrent()
        {
            foreach (var currentSlice in currentSlices)
            {
                Destroy(currentSlice.gameObject);
            }
        }
    }
}
