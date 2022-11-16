
using UnityEditor;
using UnityEngine;

namespace MyEditor
{
    [CustomPropertyDrawer(typeof(MinMaxRangeAttribute))]
    
    public class IntRangeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.serializedObject.isEditingMultipleObjects) return 0f;
            return base.GetPropertyHeight(property, label) + 16f;
        }

        public override void OnGUI(Rect position, SerializedProperty property,
            GUIContent label)
        {
            if (property.serializedObject.isEditingMultipleObjects) return;
            var minProperty = property.FindPropertyRelative("x");
            var maxProperty = property.FindPropertyRelative("y");
            
            var minmax = (this.attribute as MinMaxRangeAttribute) ?? new MinMaxRangeAttribute(0, 200);
            position.height -= 16f;

            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            
            switch (property.propertyType)
            {
                case SerializedPropertyType.Vector2Int:
                    DrawIntRange(position, minProperty, maxProperty, minmax);
                    break;
                case SerializedPropertyType.Vector2:
                    DrawFloatRange(position, minProperty, maxProperty, minmax);
                    break;
                default:
                    const float BOX_HEIGHT = 25;
                    var infoBox = new Rect(position);
                    GUI.Box(infoBox, "Invalid Property type!", EditorStyles.helpBox);
                    position.y += BOX_HEIGHT;
                    EditorGUI.PropertyField(position, property, GUIContent.none);
                    break;
            }
           
            EditorGUI.EndProperty();
        }

        private static void DrawFloatRange(Rect position, SerializedProperty minProperty, SerializedProperty maxProperty,
            MinMaxRangeAttribute minmax)
        {
            var minF = minProperty.floatValue;
            var maxF = maxProperty.floatValue;

            var left = new Rect(position.x, position.y, position.width / 2 - 11f, position.height);
            var right = new Rect(position.x + position.width - left.width, position.y, left.width, position.height);
            var mid = new Rect(left.xMax, position.y, 22, position.height);
            minF = Mathf.Clamp(EditorGUI.FloatField(left, minF), minmax.min, maxF);
            EditorGUI.LabelField(mid, " to ");
            maxF = Mathf.Clamp(EditorGUI.FloatField(right, maxF), minF, minmax.max);
            position.y += 16f;
            EditorGUI.MinMaxSlider(position, GUIContent.none, ref minF, ref maxF, minmax.min, minmax.max);
            minProperty.floatValue = minF;
            maxProperty.floatValue = maxF;
        }

        private static void DrawIntRange(Rect position, SerializedProperty minProperty, SerializedProperty maxProperty,
            MinMaxRangeAttribute minmax)
        {
            var minI = minProperty.intValue;
            var maxI = maxProperty.intValue;

            var left = new Rect(position.x, position.y, position.width / 2 - 11f, position.height);
            var right = new Rect(position.x + position.width - left.width, position.y, left.width, position.height);
            var mid = new Rect(left.xMax, position.y, 22, position.height);
            var minF = (float)minI;
            var maxF = (float)maxI;

            minF = Mathf.Clamp(EditorGUI.FloatField(left, minF), minmax.min, maxF);
            minI = Mathf.RoundToInt(minF);
            EditorGUI.LabelField(mid, " to ");
            maxF = Mathf.Clamp(EditorGUI.FloatField(right, maxF), minF, minmax.max);
            maxI = Mathf.RoundToInt(maxF);
            position.y += 16f;
            EditorGUI.MinMaxSlider(position, GUIContent.none, ref minF, ref maxF, minmax.min, minmax.max);
            minI = Mathf.RoundToInt(minF);
            maxI = Mathf.RoundToInt(maxF);
            minProperty.intValue = minI;
            maxProperty.intValue = maxI;
        }
    }


    
}