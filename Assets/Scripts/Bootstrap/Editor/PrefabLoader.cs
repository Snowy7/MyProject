using UnityEditor;
using UnityEngine;

namespace Bootstrap.Editor
{
    public class PrefabLoader : EditorWindow
    {
        // A Window to show all prefabs in the Resources/OnLoad folder and remove / add them.
        
        [MenuItem("Snowy/Bootstrap/Prefab Loader")]
        private static void ShowWindow()
        {
            GetWindow<PrefabLoader>("Prefab Loader");
        }
        
        private void OnGUI()
        {
            // If the folder does not exist make it.
            if (!System.IO.Directory.Exists(Application.dataPath + "/Resources/OnLoad"))
            {
                System.IO.Directory.CreateDirectory(Application.dataPath + "/Resources/OnLoad");
            }
            
            var resources = Resources.LoadAll("OnLoad", typeof(UnityEngine.Object));

            if (resources.Length > 0)
            {
                // Flexible grid layout with a max width of 64 and a padding of 10.
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Space(10);
                
                // Show all prefabs in the Resources/OnLoad folder with icons.
                foreach (var resource in resources)
                {
                    var texture = AssetPreview.GetAssetPreview(resource);

                    // Vertical 64x64 icon and name. centered.
                    GUILayout.BeginVertical();
                    // Show the icon centered.
                    GUILayout.Button(texture, GUILayout.Width(64), GUILayout.Height(64));
                    // Show the name centered below the icon with a max width of 64 and a ellipsis if it is too long.
                    GUILayout.Label(resource.name, GUILayout.Width(64), GUILayout.MaxWidth(64), GUILayout.Height(20), GUILayout.ExpandWidth(false));
                    // Add a button to remove the prefab from the Resources/OnLoad folder centered below the name.
                    if (GUILayout.Button("Remove", GUILayout.Width(64)))
                    {
                        // Remove the prefab from the Resources/OnLoad folder.
                        System.IO.File.Delete(Application.dataPath + "/Resources/OnLoad/" + resource.name + ".prefab");
                        
                        // Remove the meta file as well.
                        System.IO.File.Delete(Application.dataPath + "/Resources/OnLoad/" + resource.name + ".prefab.meta");
                        
                        // Refresh the editor to show the changes.
                        AssetDatabase.Refresh();
                    }
                    
                    GUILayout.EndVertical();
                }
                
                GUILayout.EndHorizontal();
            }
            else
            {
                // If there are no prefabs in the Resources/OnLoad folder show a message.
                GUILayout.Label("No Prefabs in Resources/OnLoad");
                GUILayout.Space(10);
                GUILayout.Label("You can select an object in the scene to add it to the Resources/OnLoad folder.");
            }
            
            // Check if the user has selected an object in the scene.
            if (Selection.activeGameObject != null)
            {
                // Add a button to add the selected object to the Resources/OnLoad folder.
                if (GUILayout.Button("Add Selected"))
                {
                    var prefab = PrefabUtility.SaveAsPrefabAsset(Selection.activeGameObject, "Assets/Resources/OnLoad/" + Selection.activeGameObject.name + ".prefab");
                    Resources.Load("OnLoad/" + prefab.name);
                }
            }
            
            // Add a button to add a prefab to the Resources/OnLoad folder.
            if (GUILayout.Button("Add Prefab"))
            {
                var path = EditorUtility.OpenFilePanel("Add Prefab", "Assets", "prefab");
                if (path.Length != 0)
                {
                    var prefab = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
                    Resources.Load("OnLoad/" + prefab.name);
                }
            }
        }
    }
}