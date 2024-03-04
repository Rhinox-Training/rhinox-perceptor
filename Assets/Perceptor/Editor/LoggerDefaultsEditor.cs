using Rhinox.Perceptor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Perceptor.Editor
{
    [CustomEditor(typeof(LoggerDefaults))]
    public class LoggerDefaultsEditor : UnityEditor.Editor
    {
        private bool _isActive;
        
        private SerializedObject _objectSO = null;
        private ReorderableList _listRE = null;
        private GUIStyle _style;
        
        private void OnEnable()
        {
            if (this.target is LoggerDefaults)
            {
                _objectSO = new SerializedObject(target);

                //init list
                _listRE = new ReorderableList(_objectSO, _objectSO.FindProperty("Settings"), false,
                    true, false, true);

                //handle drawing
                _listRE.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Settings");
                _listRE.drawElementCallback = DrawSettingData;
                _listRE.elementHeightCallback = CalculateElementHeight;
                _listRE.drawElementBackgroundCallback += DrawElementBackground;
            }
        }

        private const float _verticalElementPadding = 3.0f;
        private readonly Color _tileBackgroundColor = new Color(0.2196f, 0.2196f, 0.2196f, 1.0f);
        private readonly Color _altBackgroundColor = new Color(0.34f, 0.34f, 0.34f, 1.0f);
        private readonly Color _selectedBackgroundColor = new Color(0.44f, 0.44f, 0.44f, 1.0f);
        private void DrawElementBackground(Rect rect, int index, bool isactive, bool isfocused)
        {
            Color c = isfocused ? _selectedBackgroundColor : (index % 2 == 0 ? _tileBackgroundColor : _altBackgroundColor);
            EditorGUI.DrawRect(rect, c);
        }

        private float CalculateElementHeight(int index)
        {
            if (_objectSO == null)
                return EditorGUIUtility.singleLineHeight;
            
            return 3.0f * EditorGUIUtility.singleLineHeight + 2.0f * _verticalElementPadding;
        }

        private bool TryGetLoggerName(SerializedProperty loggerItem, out GUIContent name)
        {
            var typeName = loggerItem.FindPropertyRelative(nameof(LoggerSettings.TypeName));
            var fullTypeName = loggerItem.FindPropertyRelative(nameof(LoggerSettings.FullTypeName));
            if (typeName == null || fullTypeName == null)
            {
                name = GUIContent.none;
                return false;
            }

            if (fullTypeName.stringValue?.Equals("Rhinox.Perceptor.PLog+DefaultLogger") ?? false) // TODO: magic string
                name = new GUIContent($"{typeName.stringValue}");
            else
                name = new GUIContent($"{typeName.stringValue} ({fullTypeName.stringValue})");
            return true;
        }
        
        
        private void DrawSettingData(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty itemData = _listRE.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty mutedProperty = itemData.FindPropertyRelative(nameof(LoggerSettings.Muted));

            GUIContent objectLabel = new GUIContent($"Logger {index}");
            if (TryGetLoggerName(itemData, out GUIContent loggerName))
                objectLabel = loggerName;

            rect.y += _verticalElementPadding;
            mutedProperty.boolValue = !BeginToggleGroup(rect, objectLabel, !mutedProperty.boolValue);
            //using (new EditorGUILayout.ToggleGroupScope("blabla", true))
            {
                rect.y += EditorGUIUtility.singleLineHeight;
                SerializedProperty itemText = itemData.FindPropertyRelative(nameof(LoggerSettings.Level));
                SerializedProperty itemImage = itemData.FindPropertyRelative(nameof(LoggerSettings.ThrowExceptionOnFatal));

                RectOffset offset = new RectOffset(0, 0, -1, -3);
                rect = offset.Add(rect);
                rect.height = EditorGUIUtility.singleLineHeight;

                EditorGUI.PropertyField(rect, itemText, new GUIContent(itemText.displayName));
                rect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(rect, itemImage, new GUIContent(itemImage.displayName));
            }
            EndToggleGroup();
        }

        public static bool BeginToggleGroup(Rect rect, GUIContent label, bool toggle)
        {
            float cacheRect = rect.height;
            rect.height = EditorGUIUtility.singleLineHeight;
            toggle = EditorGUI.ToggleLeft(rect, label, toggle, EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(!toggle);
            GUILayout.BeginVertical();
            rect.height = cacheRect;
            return toggle;
        }

        /// <summary>
        ///   <para>Close a group started with BeginToggleGroup.</para>
        /// </summary>
        /// <footer><a href="https://docs.unity3d.com/2019.4/Documentation/ScriptReference/30_search.html?q=EditorGUILayout.EndToggleGroup">`EditorGUILayout.EndToggleGroup` on docs.unity3d.com</a></footer>
        public static void EndToggleGroup()
        {
            GUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();
        }
        
        
        public override void OnInspectorGUI()
        {
            _objectSO.Update();
            _listRE.DoLayoutList();
            _objectSO.ApplyModifiedProperties();

            // if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
            }

            GUILayout.Space(5f);
            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label($"The compiled log level is: {(target as LoggerDefaults).ActiveSymbol}", _style, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            GUILayout.Space(5f);

            if (GUILayout.Button("Populate LoggerTypes"))
            {
                LoggerDefaults.TryPopulate();
            }
        }
    }
}