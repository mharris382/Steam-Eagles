using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace InputIcons
{
    [CustomEditor(typeof(InputIconSetKeyboardSO))]
    public class InputIconSetKeyboardEditor : Editor
    {
        private ReorderableList customInputContextIconList;

        private void OnEnable()
        {
            DrawCustomContextList();
        }

#if UNITY_EDITOR
        public static void PackIconSets()
        {
            InputIconsSpritePacker.PackIconSets();
        }

        public static void PackIconSet(InputIconSetBasicSO iconSet)
        {
            InputIconsSpritePacker.PackIconSet(iconSet);
        }

#endif
        public override void OnInspectorGUI()
        {
            InputIconSetKeyboardSO iconSet = (InputIconSetKeyboardSO)target;


            iconSet.iconSetName = EditorGUILayout.TextField("Icon Set Name", iconSet.iconSetName);
            iconSet.deviceDisplayColor = EditorGUILayout.ColorField("Device Display Color", iconSet.deviceDisplayColor);


            EditorGUILayout.Space(5);

            if (GUILayout.Button("Pack to Sprite Asset", GUILayout.Height(30)))
            {
                PackIconSet(iconSet);
                return;
            }
            EditorGUILayout.Space(5);

            if (GUILayout.Button("Initialize Mouse Buttons", GUILayout.Height(30)))
            {
                iconSet.InitializeMouseButtons();
            }
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("To change the used sprites more conveniently, drag this icon set into a folder containing sprites. " +
                "Then click the button below to try to automatically find corresponding sprites." +
                "Check if all sprites were found. \nNote: Custom context icons must be assigned manually. \nThen use the \"Pack to Sprite Asset\" button above.", MessageType.Info);
            if (GUILayout.Button("Automatically apply button sprites of folder", GUILayout.Height(30)))
            {
                iconSet.TryGrabSprites();
            }

          
            iconSet.unboundData = DrawDeviceField(iconSet.unboundData);
            iconSet.fallbackData = DrawDeviceField(iconSet.fallbackData);

            EditorGUILayout.LabelField("Icons - Custom Contexts", EditorStyles.boldLabel);
            customInputContextIconList.DoLayoutList();

            EditorGUILayout.LabelField("Icons - Mouse", EditorStyles.boldLabel);

            iconSet.mouse = DrawDeviceField(iconSet.mouse);
            iconSet.mouse_left = DrawDeviceField(iconSet.mouse_left);
            iconSet.mouse_right = DrawDeviceField(iconSet.mouse_right);
            iconSet.mouse_middle = DrawDeviceField(iconSet.mouse_middle);

            EditorGUILayout.LabelField("Icons - Keys", EditorStyles.boldLabel);

            for (int i = 0; i < iconSet.inputKeys.Count; i++)
            {
                iconSet.inputKeys[i] = DrawDeviceField(iconSet.inputKeys[i]);
            }


            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(iconSet);


        }

        private InputSpriteData DrawDeviceField(InputSpriteData data)
        {

            EditorGUILayout.LabelField(data.GetButtonName());

            EditorGUILayout.BeginHorizontal(GUILayout.Width(200));

            EditorGUIUtility.labelWidth = 40;
            EditorGUIUtility.fieldWidth = 50;
            data.sprite = (Sprite)EditorGUILayout.ObjectField("Sprite", data.sprite, typeof(Sprite), false);
            GUI.enabled = false;
            EditorGUILayout.Space(30);
            EditorGUIUtility.labelWidth = 130;
            EditorGUIUtility.fieldWidth = 80;
            data.textMeshStyleTag = EditorGUILayout.TextField("TextMeshStyleTag", data.textMeshStyleTag);
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            InputSpriteData d = new InputSpriteData(data.GetButtonName(), data.sprite, data.textMeshStyleTag);

            return d;
        }

        Rect CalculateColumn(Rect rect, int columnNumber, float xPadding, float xWidth)
        {
            float xPosition = rect.x;
            switch (columnNumber)
            {
                case 1:
                    xPosition = rect.x + xPadding;
                    break;

                case 2:
                    xPosition = rect.x + rect.width / 2 + xPadding;
                    break;
            }


            return new Rect(xPosition, rect.y, rect.width / 2 - xWidth, EditorGUIUtility.singleLineHeight);
        }

        void DrawCustomContextList()
        {
            customInputContextIconList = new ReorderableList(serializedObject, serializedObject.FindProperty("customContextIcons"), true, true, true, true);

            customInputContextIconList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(new Rect(rect.x + 20, rect.y, 140, EditorGUIUtility.singleLineHeight), "Input Binding String");
                EditorGUI.LabelField(new Rect(rect.x + 175, rect.y, 100, EditorGUIUtility.singleLineHeight), "Display Icon");
            };

            customInputContextIconList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {

                var element = customInputContextIconList.serializedProperty.GetArrayElementAtIndex(index);

                rect.y += 2;

                EditorGUI.PropertyField(new Rect(rect.x + 5, rect.y, 140, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("textMeshStyleTag"), GUIContent.none);
                EditorGUI.PropertyField(new Rect(rect.x + 160, rect.y, 170, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("customInputContextSprite"), GUIContent.none);


            };
        }
    }
}