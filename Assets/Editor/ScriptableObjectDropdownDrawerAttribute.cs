#if UNITY_EDITOR


using Attributes;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomPropertyDrawer(typeof(ScriptableObjectDropdownAttribute))]
    public class ScriptableObjectDropdownDrawerAttribute : PropertyDrawer
    {
        private UnityEditor.Editor _editor;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, label, true);

            if (property.objectReferenceValue != null)
            {
                property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, GUIContent.none);
            }

            if (!property.isExpanded) return;
        
            EditorGUI.indentLevel++;

            if (!_editor)
            {
            
                UnityEditor.Editor.CreateCachedEditor(property.objectReferenceValue, null, ref _editor);
            }
            _editor.OnInspectorGUI();

            EditorGUI.indentLevel--;
        }
    }
}
    


#endif
