using Menu.Attribute;
using Toolbox.Editor.Drawers;
using UnityEditor;
using UnityEngine;

namespace Menu.Editor
{
    [CustomPropertyDrawer(typeof(MenuIndexAttribute))]
    public class MenuIndexAttributeDrawer : PropertyDrawerBase
    {
        protected override void OnGUISafe(Rect position, SerializedProperty property, GUIContent label)
        {
            var manager = MenuManager.Instance;
            
            if (manager == null)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            var menus = manager.menus;

            var menuNames = new string[menus.Count];
            for (var i = 0; i < menus.Count; i++)
            {
                menuNames[i] = menus[i].menuName;
            }
            
            var index = property.intValue;
            
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);
            
            EditorGUI.BeginChangeCheck();
            index = EditorGUI.Popup(position, index, menuNames);
            if (EditorGUI.EndChangeCheck())
            {
                property.intValue = index;
            }
            
            EditorGUI.EndProperty();
        }


        public override bool IsPropertyValid(SerializedProperty property)
        {
            
            return property.propertyType == SerializedPropertyType.Integer && MenuManager.Instance;
        }
    }
}