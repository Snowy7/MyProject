using System.Collections.Generic;
using Toolbox.Editor;
using Toolbox.Editor.Wizards;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Menu.Editor
{
    public class MenuCreator : ToolboxWizard
    {
        internal const string kFloatArgument  = "m_FloatArgument";
        internal const string kIntArgument    = "m_IntArgument";
        internal const string kObjectArgument = "m_ObjectArgument";
        internal const string kStringArgument = "m_StringArgument";
        internal const string kBoolArgument = "m_BoolArgument";
        internal const string kObjectArgumentAssemblyTypeName = "m_ObjectArgumentAssemblyTypeName";
        
        [SerializeField] private MenuObject menuPrefab;
        [SerializeField] private SnButton buttonPrefab;
        
        private MenuObject.MenuData menuData = new MenuObject.MenuData();
        public List<SnButton.ButtonData> buttonData;
        
        bool canEditManager;
        [SerializeField] MenuManager menuManager;
        
        // 
        private SerializedObject m_serializedObject;
        private SerializedObject serializedObject
        {
            get
            {
                if (m_serializedObject == null)
                {
                    m_serializedObject = new SerializedObject(this);
                }
                return m_serializedObject;
            }
        }
        
        public List<UnityEvent> events = new List<UnityEvent>();
        
        [MenuItem("GameObject/Menu/Create Menu Manager", false, 0)]
        public static void CreateMenuManager()
        {
            // load the menu manager from the "../Resources/Menu" folder
            var menuManager = Resources.Load<MenuManager>("Menu/MenuManager");
            
            // if the menu manager is null
            if (menuManager)
            {
                // Instantiate a new menu manager
                var manager = Instantiate(menuManager);
                // Set the name
                manager.name = "Menu Manager";
                
                // Get canvas and set the camera
                var canvas = manager.GetComponentInChildren<Canvas>();
                if (canvas)
                {
                    canvas.worldCamera = Camera.main;
                }
                
                // Log a message
                Debug.Log("Menu Manager created");
                
                // Select the menu manager
                Selection.activeGameObject = manager.gameObject;
                
                // Open the menu creator
                DisplayWizard<MenuCreator>("Create Menu");
            }

            // if the menu manager is not null
            else
            {
                // Log an error
                Debug.LogError("No Menu Manager found in Resources/Menu folder");
            }
        }
        
        // Create a new menu
        [MenuItem("GameObject/Menu/Create Menu", false, 10)]
        public static void CreateMenu()
        {
            // Try to find the menu manager
            var menuManager = FindFirstObjectByType<MenuManager>();
            if (!menuManager)
            {
                // Display a text box asking the user to create a menu manager
                if (EditorUtility.DisplayDialog("No Menu Manager Found",
                        "No Menu Manager found in the scene. Would you like to create one?", "Yes", "No"))
                {
                    CreateMenuManager();
                    return;
                }
            }
            
            DisplayWizard<MenuCreator>("Create Menu");
        }

        protected override void OnWizardCreate()
        {
            base.OnWizardCreate();

            if (!menuManager)
            {
                menuManager = FindFirstObjectByType<MenuManager>();
            }
            
            // Create the menu
            var menu = Instantiate(menuPrefab, menuManager.transform);
            menu.LoadMenuData(menuData);
            menu.name = menuData.menuTitle;
            menu.gameObject.SetActive(false);
            
            // Add the menu to the menu manager
            menuManager.AddMenu(menu);
            
            // Add the buttons
            for (var i = 0; i < buttonData.Count; i++)
            {
                buttonData[i].behaviourEvent = events[i];
                buttonData[i].ListerArguments = new List<(int, object)>();
                SerializedProperty property = serializedObject.FindProperty("events").GetArrayElementAtIndex(i);
                var array = property.FindPropertyRelative("m_PersistentCalls.m_Calls.Array");

                for (int x = 0; x < array.arraySize; x++)
                {
                    buttonData[i].ListerArguments.Add((0, null));
                    var element = array.GetArrayElementAtIndex(x);
                    var mode = element.FindPropertyRelative("m_Mode").enumValueIndex;
                    var arguments = element.FindPropertyRelative("m_Arguments");
                    switch ((PersistentListenerMode) mode)
                    {
                        case PersistentListenerMode.Bool:
                            var boolValue = arguments.FindPropertyRelative(kBoolArgument).boolValue;
                            buttonData[i].ListerArguments[x] = (mode, boolValue);
                            break;
                        case PersistentListenerMode.Float:
                            var floatValue = arguments.FindPropertyRelative(kFloatArgument).floatValue;
                            buttonData[i].ListerArguments[x] = (mode, floatValue);
                            break;
                        
                        case PersistentListenerMode.Int:
                            var intValue = arguments.FindPropertyRelative(kIntArgument).intValue;
                            buttonData[i].ListerArguments[x] = (mode, intValue);
                            break;
                        
                        case PersistentListenerMode.Object:
                            var objectValue = arguments.FindPropertyRelative(kObjectArgument).objectReferenceValue;
                            buttonData[i].ListerArguments[x] = (mode, objectValue);
                            break;
                        
                        case PersistentListenerMode.String:
                            var stringValue = arguments.FindPropertyRelative(kStringArgument).stringValue;
                            buttonData[i].ListerArguments[x] = (mode, stringValue);
                            break;
                        case PersistentListenerMode.Void:
                        default:
                            buttonData[i].ListerArguments[x] = (mode, null);
                            break;
                    }
                }
                
                menu.AddButton(buttonPrefab, buttonData[i]);
            }
        }

        protected override void OnWizardGui()
        {
            canEditManager = EditorGUILayout.Toggle("Edit Menu Manager", canEditManager);
            
            GUI.enabled = canEditManager;
            menuManager = EditorGUILayout.ObjectField("Menu Manager", menuManager, typeof(MenuManager), true) as MenuManager;
            GUI.enabled = true;
            
            // Show the menu manager field disabled
            if (!menuManager)
            {
                menuManager = FindFirstObjectByType<MenuManager>();
            }
            
            if (!menuManager)
            {
                EditorGUILayout.HelpBox("No Menu Manager found in the scene", MessageType.Error);

                if (GUILayout.Button("Create Menu Manager"))
                {
                    CreateMenuManager();
                }
                
                return;
            } 
            
            // by default show prefab fields 
            Rect rect = EditorGUILayout.GetControlRect();

            // Title
            ToolboxEditorGui.BoldLabel(rect, "Menu Creator");

            // Menu Title
            menuPrefab =
                EditorGUILayout.ObjectField("Menu Prefab", menuPrefab, typeof(MenuObject), false) as MenuObject;
            ToolboxEditorGui.DrawLine();
            
            // Menu Title
            EditorGUILayout.Space();
            if (menuData == null)
            {
                menuData = new MenuObject.MenuData();
            }
            
            // Menu Title
            menuData.menuTitle = EditorGUILayout.TextField("Menu Title", menuData.menuTitle);
            // Spacing
            menuData.spacing = EditorGUILayout.IntField("Spacing", menuData.spacing);
            // Is Default
            menuData.isDefault = EditorGUILayout.Toggle("Is Default", menuData.isDefault);
            // Has Back Button
            menuData.hasBackButton = EditorGUILayout.Toggle("Has Back Button", menuData.hasBackButton);


            // Button List
            ToolboxEditorGui.DrawLine();
            
            // Button Prefab
            buttonPrefab =
                EditorGUILayout.ObjectField("Button Prefab", buttonPrefab, typeof(SnButton), false) as SnButton;
            
            EditorGUILayout.Space();
            
            if (buttonData == null)
            {
                buttonData = new List<SnButton.ButtonData>();
            }
            
            rect = EditorGUILayout.GetControlRect();
            
            // Button List
            ToolboxEditorGui.BoldLabel(rect, "Button List");
            
            if (buttonData.Count == 0)
            {
                // Info box
                EditorGUILayout.HelpBox("No buttons added", MessageType.Info);
            }
            else
            {
                // Box
                EditorGUILayout.BeginVertical("Box");
                
                EditorGUI.indentLevel+=2;

                for (int i = 0; i < buttonData.Count; i++)
                {
                    // Foldout
                    // Box
                    EditorGUILayout.BeginVertical("Box");
                    buttonData[i].foldout = EditorGUILayout.Foldout(buttonData[i].foldout, "Button " + i);
                    if (buttonData[i].foldout)
                    {
                        
                        // Text
                        buttonData[i].text = EditorGUILayout.TextField("Text", buttonData[i].text);
                        // Image
                        buttonData[i].image =
                            EditorGUILayout.ObjectField("Image", buttonData[i].image, typeof(Sprite), false) as Sprite;
                        // Interactable
                        buttonData[i].interactable = EditorGUILayout.Toggle("Interactable", buttonData[i].interactable);
                        
                        buttonData[i].useCustomTheme = EditorGUILayout.Toggle("Use Custom Theme", buttonData[i].useCustomTheme);
                        
                        if (buttonData[i].useCustomTheme)
                        {
                            // Primary Color
                            buttonData[i].primaryColor =
                                EditorGUILayout.ColorField("Primary Color", buttonData[i].primaryColor);
                            // Secondary Color
                            buttonData[i].secondaryColor =
                                EditorGUILayout.ColorField("Secondary Color", buttonData[i].secondaryColor);
                            
                        }
                        
                        // Play Sound
                        buttonData[i].playSound = EditorGUILayout.Toggle("Play Sound", buttonData[i].playSound);
                        // Hover Sound
                        buttonData[i].hoverSound =
                            EditorGUILayout.ObjectField("Hover Sound", buttonData[i].hoverSound, typeof(AudioClip),
                                false) as AudioClip;
                        // Click Sound
                        buttonData[i].clickSound =
                            EditorGUILayout.ObjectField("Click Sound", buttonData[i].clickSound, typeof(AudioClip),
                                false) as AudioClip;
                        
                        // Behaviour
                        buttonData[i].behaviour = (SnButton.Behaviour) EditorGUILayout.EnumPopup("Behaviour", buttonData[i].behaviour);
                        
                        // Behaviour Data
                        switch (buttonData[i].behaviour)
                        {
                            case SnButton.Behaviour.None:
                                break;
                            case SnButton.Behaviour.Quit:
                                break;
                            case SnButton.Behaviour.LoadScene:
                                // check if is null or not string
                                if (buttonData[i].BehaviourData == null || !(buttonData[i].BehaviourData is string))
                                {
                                    buttonData[i].BehaviourData = "";
                                }
                                buttonData[i].BehaviourData = EditorGUILayout.TextField("Scene Name", (string) buttonData[i].BehaviourData);
                                break;
                            case SnButton.Behaviour.OpenMenu:
                                if (buttonData[i].BehaviourData == null || !(buttonData[i].BehaviourData is int))
                                {
                                    buttonData[i].BehaviourData = 0;
                                }
                                // draw a int field
                                buttonData[i].BehaviourData = EditorGUILayout.Popup((int)buttonData[i].BehaviourData, GetMenuNames());
                                break;
                            case SnButton.Behaviour.Custom:
                                // Draw space
                                EditorGUILayout.Space();
                                // Draw the unity event field from the list
                                SerializedProperty property = serializedObject.FindProperty("events").GetArrayElementAtIndex(i);
                                // On Add   
                                EditorGUILayout.PropertyField(property);
                                break;
                        }

                        // Space
                        EditorGUILayout.Space();
                        
                        // Centered remove button
                        rect = EditorGUILayout.GetControlRect();
                        rect.x += rect.width / 2 - 50;
                        rect.width = 100;
                        if (GUI.Button(rect, "Remove Button"))
                        {
                            events.RemoveAt(i);
                            buttonData.RemoveAt(i);
                        }
                    }
                    
                    EditorGUILayout.EndVertical();

                }
                
                EditorGUI.indentLevel-=2;
                
                EditorGUILayout.EndVertical();
            }

            // Space
            EditorGUILayout.Space();
            
            // Add Button
            if (GUILayout.Button("Add Button"))
            {
                buttonData.Add(buttonPrefab.GetButtonData());
                events.Add(new UnityEvent());
            }
        }

        private string[] GetMenuNames()
        {
            if (!menuManager)
            {
                menuManager = FindFirstObjectByType<MenuManager>();
            }
            
            var menuNames = new string[menuManager.menus.Count];
            for (int i = 0; i < menuManager.menus.Count; i++)
            {
                menuNames[i] = menuManager.menus[i].menuName;
            }
            
            return menuNames;
        }
        
    }
}