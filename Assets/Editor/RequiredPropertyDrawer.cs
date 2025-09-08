#if UNITY_EDITOR

using Attributes;
using UnityEditor;
using UnityEngine;
using Logger = General.Logger;

namespace Editor
{
    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    public class RequiredPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            RequiredAttribute requiredAttribute = (RequiredAttribute)attribute;
        
            // Force the property field to use only single line height
            Rect propertyRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        
            // Use BeginProperty/EndProperty to ensure proper behavior
            EditorGUI.BeginProperty(propertyRect, label, property);
            EditorGUI.PropertyField(propertyRect, property, label);
            EditorGUI.EndProperty();
        
            // Show error below
            if (property.objectReferenceValue != null) return;
            Logger.LogError($"{property.name} is required but not assigned in the inspector.");
            Rect helpBoxRect = new Rect(
                position.x, 
                position.y + EditorGUIUtility.singleLineHeight + 2,
                position.width, 
                EditorGUIUtility.singleLineHeight);
                
            EditorGUI.HelpBox(helpBoxRect, requiredAttribute.ErrorMessage, MessageType.Error);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight; // Property field height
        
            if (property.objectReferenceValue == null)
            {
                height += EditorGUIUtility.singleLineHeight + 2; // Error message height
            }
        
            return height;
        }
    }
}
#endif