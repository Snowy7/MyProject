using Toolbox.Editor.Drawers;
using UnityEditor;
using UnityEngine;

namespace Utils.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(AnimatorStateAttribute))]
    public class AnimatorStateAttributeDrawer : PropertyDrawerBase
    {
        protected override void OnGUISafe(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = (AnimatorStateAttribute) attribute;
            var animatorRef = property.serializedObject.FindProperty(attr.animatorRef);
            
            if (animatorRef == null)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            var animator = animatorRef.objectReferenceValue as Animator;
            
            if (animator == null)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            var clips = animator.runtimeAnimatorController.animationClips;
            var clipNames = new string[clips.Length];
            for (var i = 0; i < clips.Length; i++)
            {
                clipNames[i] = clips[i].name;
            }
            
            var index = ArrayUtility.FindIndex(clipNames, clipName => clipName == property.stringValue);
            
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);
            
            EditorGUI.BeginChangeCheck();
            index = Mathf.Clamp(index, 0, clipNames.Length - 1);
            index = EditorGUI.Popup(position, index, clipNames);
            index = Mathf.Clamp(index, 0, clipNames.Length - 1);
            
            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = clipNames[index];
            }
            
            EditorGUI.EndProperty();
        }


        public override bool IsPropertyValid(SerializedProperty property)
        {
            var attr = (AnimatorStateAttribute) attribute;
            var animatorRef = property.serializedObject.FindProperty(attr.animatorRef);
            return property.propertyType == SerializedPropertyType.String && animatorRef != null;
        }
    }
}