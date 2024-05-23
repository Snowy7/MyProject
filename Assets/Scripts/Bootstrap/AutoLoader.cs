using UnityEngine;

namespace Bootstrap
{
    public class AutoLoader
    {
        // Auto load everything in the Resources/OnLoad folder
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoLoad()
        {
            // If the folder does not exist make it.
            if (!System.IO.Directory.Exists(Application.dataPath + "/Resources/OnLoad"))
            {
                System.IO.Directory.CreateDirectory(Application.dataPath + "/Resources/OnLoad");
            }
            
            var resources = Resources.LoadAll("OnLoad", typeof(UnityEngine.Object));
            foreach (var resource in resources)
            {
                UnityEngine.Object.Instantiate(resource);
            }
        }
    }
}