using System.Collections.Generic;
using UnityEngine;

namespace Snowy.Settings
{
    public class Settings : MonoBehaviour
    {
        private static Settings _instance;
        
        public static Settings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<Settings>();
                }
                return _instance;
            }
        }
        
        public DisplaySettings displaySettings;
        public List<GraphicProfile> graphicSettings = new();
        public AudioSettings audioSettings;
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            displaySettings ??= new DisplaySettings();
            audioSettings ??= new AudioSettings();
            
            displaySettings.Load();
            audioSettings.Load();
            foreach (GraphicProfile profile in graphicSettings) profile.Load();
        }
        
        # if UNITY_EDITOR
        private void OnValidate()
        {
            audioSettings ??= new AudioSettings();
        }
        
        #endif
        
        #region Graphic Settings
        
        public void AddProfile(GraphicProfile profile)
        {
            graphicSettings.Add(profile);
        }
        public GraphicProfile GetProfile(string profileName)
        {
            return graphicSettings.Find(x => x.name == profileName);
        }
        
        public void SelectProfile(string profileName)
        {
            GraphicProfile profile = GetProfile(profileName);
            if (profile != null)
            {
                profile.Apply();
            }
        }
        
        #endregion
        
        #region Audio Settings
        
        public void AddAudioProfile(AudioProfile profile)
        {
            audioSettings.audioProfiles.Add(profile);
        }
        
        #endregion
    }
}