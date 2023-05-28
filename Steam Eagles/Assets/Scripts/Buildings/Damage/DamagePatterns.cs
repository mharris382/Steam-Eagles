using System;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
#endif

namespace Buildings.Damage
{
    [CreateAssetMenu(fileName = "New Damage Pattern Set", menuName = "Steam Eagles/New Damage Patterns Library", order = 100)]
    public class DamagePatterns : SerializedScriptableObject
    {

        public DamageGridPattern pattern;
        
        [Serializable]
        public class DamageGridPattern
        {
            private const int MIN_SIZE = 2;
            private const int MAX_SIZE = 16;
            
            
            
            [Range(MIN_SIZE, MAX_SIZE), OnValueChanged(nameof(OnSizeChanged))]public int x = 4;
            [Range(MIN_SIZE, MAX_SIZE), OnValueChanged(nameof(OnSizeChanged))]public int y = 4;
            [SerializeField]
            [Range(0, 1)]public int[,] pattern = new int[4,4]
            {
                {0, 0, 0, 0},
                {0, 1, 1, 0},
                {0, 1, 1, 0},
                {0, 0, 0, 0}
            };

            void OnSizeChanged()
            {
                var newPattern = new int[x, y];
                for (int i = 0; i < Mathf.Min(x, pattern.GetLength(0)); i++)
                {
                    for (int j = 0; j < Mathf.Min(y, pattern.GetLength(1)); j++)
                    {
                        newPattern[i, j] = pattern[i, j];
                    }
                }
                pattern = newPattern;                
            }

            private int MaxValue => Mathf.Max(x, y);
            
#if UNITY_EDITOR
            public int DrawCell(Rect bounds, int value)
            {
                if (Event.current.type == EventType.MouseDown &&
                    bounds.Contains(Event.current.mousePosition))
                {
                    if (Event.current.button == 0)
                    {
                        value++;
                        value %= MaxValue;
                    }
                    else if (Event.current.button == 1)
                    {
                        value--;
                        if (value == -1)
                        {
                            value = MaxValue-1;
                        }
                    }
                }
                float t = (float)value / MaxValue;
                
                EditorGUI.DrawRect(bounds, value > 0 ? Color.Lerp(StartColor, EndColor, t) : Color.white);
                GUI.Label(bounds, value.ToString(), new GUIStyle()
                {
                    alignment = TextAnchor.MiddleCenter
                });
                return value;
            }
            Color EndColor
            {
                get
                {
                    if (ColorUtility.TryParseHtmlString("f56641", out var c))
                    {
                        return c;
                    }
                    else
                    {
                        return new Color(1f, .6f, .2f);
                    }
                }
            }
            Color StartColor
            {
                get
                {
                    if (ColorUtility.TryParseHtmlString("f56641", out var c))
                    {
                        return c;
                    }
                    else
                    {
                        return new Color(1f, .1f, .02f);
                    }
                }
            }
#endif
        }
    }
    
    
    
}