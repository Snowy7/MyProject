using Toolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Menu.Editor
{
    [CustomEditor(typeof(MenuManager))]
    public class MenuManagerEditor : ToolboxEditor
    {
        
        // add to EditorStyles.miniPullDown to make the width of the popup smaller
        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();
            var manager = (MenuManager) target;
            if (manager.menus == null)
            {
                return;
            }
            
            // Check if all the menus are valid
            for (int i = 0; i < manager.menus.Count; i++)
            {
                if (manager.menus[i].menuObject == null)
                {
                    manager.menus.RemoveAt(i);
                }
            }
            
            // Check if there is a menu that is not in the list
            foreach (var menu in manager.transform.GetComponentsInChildren<MenuObject>())
            {
                if (!manager.menus.Exists(m => m.menuObject == menu))
                {
                    manager.AddMenu(menu);
                }
            }
            
            // Show a popup with the available menus for the default menu
            var defaultMenuProperty = serializedObject.FindProperty("defaultMenu");
            // Ignore the defaultMenu property
            // IgnoreProperty(defaultMenuProperty);
            var position = EditorGUILayout.GetControlRect();
            var (defaultIndex, defaultMenu) = manager.GetDefaultMenu();
            
            // Horizontal layout for the popup and the button
            EditorGUILayout.BeginHorizontal();
            // Label for the popup
            EditorGUILayout.LabelField("Default Menu");
            // if empty show a message
            if (manager.menus.Count == 0)
            {
                EditorGUILayout.LabelField("No menus available");
            }
            else
            {
                // draw a int field
                int index = EditorGUILayout.Popup(defaultIndex, GetMenuNames());
                // if the index is different from the default index
                if (index != defaultIndex)
                {
                    manager.SetDefaultMenu(index);
                }
            }
            EditorGUILayout.EndHorizontal();
            
            // Button to create a new menu
            if (GUILayout.Button("Create Menu"))
            {
                MenuCreator.CreateMenu();
            }
        }
        
        private string[] GetMenuNames()
        {
            var menuManager = (MenuManager) target;
            var menuNames = new string[menuManager.menus.Count];
            for (int i = 0; i < menuManager.menus.Count; i++)
            {
                menuNames[i] = menuManager.menus[i].menuName;
            }
            
            return menuNames;
        }
    }
}